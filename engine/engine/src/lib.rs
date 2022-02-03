//! This is the common engine crate. It defines all functionality the engine
//! libraries must fulfill as a standard rust crate. Other creates in this
//! workspace re-export this as a static or dynamic library.

use std::ffi::{CStr, CString};
use std::mem;
use std::os::raw::c_char;
use std::sync::{Arc, RwLock};
use std::sync::mpsc::{self, Sender};
use std::thread;

use sudoku_variants::{Sudoku, SudokuGrid};
use sudoku_variants::constraint::{Constraint, DefaultConstraint};
use sudoku_variants::generator::{Generator, Reducer};
use sudoku_variants::solver::{Solution, Solver};
use sudoku_variants::solver::strategy::{
    BoundedCellsBacktrackingStrategy,
    BoundedOptionsBacktrackingStrategy,
    CompositeStrategy,
    NakedSingleStrategy,
    NoStrategy,
    OnlyCellStrategy,
    StrategicBacktrackingSolver,
    StrategicSolver,
    TupleStrategy
};

/// A dummy solver which always returns `Solution::Amibguous`. This is used as
/// a lower bound for the easiest difficulty to prevent special cases.
struct NotSolver;

impl Solver for NotSolver {
    fn solve<C>(&self, _: &Sudoku<C>) -> Solution
    where
        C: Constraint + Clone + 'static
    {
        Solution::Ambiguous
    }
}

/// A dummy solver which always returns a `Solution::Unique`, which is probably
/// false. This is used as an upper bound for the hardest difficulty to prevent
/// special cases.
struct DummyPerfectSolver;

impl Solver for DummyPerfectSolver {
    fn solve<C>(&self, _: &Sudoku<C>) -> Solution
    where
        C: Constraint + Clone + 'static
    {
        Solution::Unique(SudokuGrid::new(1, 1).unwrap())
    }
}

fn difficulty_0() -> impl Solver {
    NotSolver
}

fn difficulty_inf() -> impl Solver {
    DummyPerfectSolver
}

fn default_difficulty_1() -> impl Solver {
    StrategicSolver::new(OnlyCellStrategy)
}

fn default_difficulty_2() -> impl Solver {
    StrategicSolver::new(
        CompositeStrategy::new(
            CompositeStrategy::new(
                OnlyCellStrategy,
                NakedSingleStrategy
            ),
            BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(0), NoStrategy)
        )
    )
}

fn default_difficulty_3() -> impl Solver {
    StrategicSolver::new(CompositeStrategy::new(
        CompositeStrategy::new(
            OnlyCellStrategy,
            NakedSingleStrategy
        ),
        CompositeStrategy::new(
            BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(1),
                BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(0), NoStrategy)),
            TupleStrategy::new(|_| 2)
        )
    ))
}

fn default_difficulty_4() -> impl Solver {
    StrategicSolver::new(
        CompositeStrategy::new(
            CompositeStrategy::new(
                OnlyCellStrategy,
                NakedSingleStrategy
            ),
            CompositeStrategy::new(
                CompositeStrategy::new(
                    BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(1),
                        BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(0), NoStrategy)),
                    TupleStrategy::new(|_| 3)
                ),
                CompositeStrategy::new(
                    BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(2),
                        CompositeStrategy::new(
                            OnlyCellStrategy,
                            NakedSingleStrategy
                        )
                    ),
                    BoundedOptionsBacktrackingStrategy::new(|_| 2, |_| Some(2),
                        CompositeStrategy::new(
                            OnlyCellStrategy,
                            NakedSingleStrategy
                        )
                    )
                )
            )
        )
    )
}

fn default_difficulty_5() -> impl Solver {
    StrategicBacktrackingSolver::new(
        CompositeStrategy::new(
            OnlyCellStrategy,
            NakedSingleStrategy
        )
    )
}

fn default_constraint() -> DefaultConstraint {
    DefaultConstraint
}

fn can_solve<C, S>(sudoku: &Sudoku<C>, solver: &S) -> bool
where
    C: Constraint + Clone + 'static,
    S: Solver
{
    matches!(solver.solve(sudoku), Solution::Unique(_))
}

fn gen_with_difficulty_thread<SL, SU, SG, C, FC>(
    lower_difficulty_bound_solver: SL,
    upper_difficulty_bound_solver: SU, generator_solver: SG,
    constraint_cons: FC, shall_continue: Arc<RwLock<bool>>,
    result_sender: Sender<Sudoku<C>>)
where
    SL: Solver,
    SU: Solver,
    SG: Solver,
    C: Constraint + Clone + 'static,
    FC: Fn() -> C
{
    let mut generator = Generator::new_default();
    let mut reducer = Reducer::new(generator_solver, rand::thread_rng());

    while *shall_continue.read().unwrap() {
        let constraint = constraint_cons();
        let mut sudoku = generator.generate(3, 3, constraint).unwrap();
        reducer.reduce(&mut sudoku);

        if can_solve(&sudoku, &lower_difficulty_bound_solver) {
            continue;
        }

        if !can_solve(&sudoku, &upper_difficulty_bound_solver) {
            continue;
        }

        result_sender.send(sudoku).unwrap();
        break;
    }
}

/// Generates a Sudoku with a specific difficulty, determined by the provided
/// solvers. Input parameters should be provided in a way that prevents
/// generation to take too long, i.e. the generator solver should be able to
/// yield a valid Sudoku quickly and the probability that it lies within the
/// difficulty bounds should be reasonably high.
///
/// # Arguments
///
/// * `lower_difficulty_bound_solver_cons`: A closure that creates a solver
/// which determines an exclusive lower bound on the difficulty. That is, it
/// shall not be able to solve the returned Sudoku.
/// * `upper_difficulty_bound_solver_cons`: A closure that creates a solver
/// which determines an inclusive upper bound on the difficulty. That is, it
/// shall be able to solve the returned Sudoku.
/// * `generator_solver_cons`: A closure that creates a solver used for
/// reducing a generated Sudoku.
/// * `constraint_cons`: A closure that creates the constraint of the generated
/// Sudoku.
fn gen_with_difficulty<SL, FSL, SU, FSU, SG, FSG, C, FC>(
    lower_difficulty_bound_solver_cons: FSL,
    upper_difficulty_bound_solver_cons: FSU, generator_solver_cons: FSG,
    constraint_cons: FC) -> Sudoku<C>
where
    SL: Solver + Send + 'static,
    FSL: Fn() -> SL,
    SU: Solver + Send + 'static,
    FSU: Fn() -> SU,
    SG: Solver + Send + 'static,
    FSG: Fn() -> SG,
    C: Constraint + Clone + Send + 'static,
    FC: Fn() -> C + Send + Copy + 'static
{
    // TODO replace with thread::available_parallelism once stable
    let threads = num_cpus::get();
    let shall_continue = Arc::new(RwLock::new(true));
    //let mut join_handles = Vec::new();
    let (sender, receiver) = mpsc::channel();

    for _ in 0..threads {
        let shall_continue = Arc::clone(&shall_continue);
        let result_sender = Sender::clone(&sender);
        let lower_difficulty_bound_solver = lower_difficulty_bound_solver_cons();
        let upper_difficulty_bound_solver = upper_difficulty_bound_solver_cons();
        let generator_solver = generator_solver_cons();
        thread::spawn(move || gen_with_difficulty_thread(
            lower_difficulty_bound_solver,
            upper_difficulty_bound_solver, generator_solver,
            constraint_cons, shall_continue, result_sender));
    }

    drop(sender);

    let result = receiver.recv().unwrap();
    let mut flag_guard = shall_continue.write().unwrap();
    *flag_guard = true;
    drop(flag_guard);

    /*for join_handle in join_handles {
        join_handle.join().unwrap();
    }*/

    result
}

/// Generates a 9x9 Sudoku with a default constraint and returns its CBOR
/// serialization.
#[no_mangle]
pub extern fn gen_default(difficulty: i32) -> *const c_char {
    let sudoku = match difficulty {
        1 => gen_with_difficulty(
            difficulty_0,
            default_difficulty_1,
            default_difficulty_1,
            default_constraint),
        2 => gen_with_difficulty(
            default_difficulty_1,
            default_difficulty_2,
            default_difficulty_2,
            default_constraint),
        3 => gen_with_difficulty(
            default_difficulty_2,
            default_difficulty_3,
            default_difficulty_5,
            default_constraint),
        4 => gen_with_difficulty(
            default_difficulty_3,
            default_difficulty_4,
            default_difficulty_5,
            default_constraint),
        5 => gen_with_difficulty(
            default_difficulty_4,
            difficulty_inf,
            default_difficulty_5,
            default_constraint),
        _ => panic!("Invalid difficulty: {}", difficulty)
    };

    let json = serde_json::to_string(&sudoku).unwrap();
    let json_c = CString::new(json).unwrap();
    let json_ptr = json_c.as_ptr();
    mem::forget(json_c);
    json_ptr
}

/// Checks whether all constraints in a 9x9 Sudoku with a default constraint
/// are satisfied.
#[no_mangle]
pub extern fn check_default(json: *const c_char) -> bool {
    let json = unsafe { CStr::from_ptr(json) }.to_str().unwrap();
    let sudoku: Sudoku<DefaultConstraint> =
        serde_json::from_str(json).unwrap();
    sudoku.is_valid()
}

/// Returns 42. For tests that the library was loaded correctly.
#[no_mangle]
pub extern fn test() -> i32 {
    42
}
