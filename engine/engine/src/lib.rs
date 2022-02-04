//! This is the common engine crate. It defines all functionality the engine
//! libraries must fulfill as a standard rust crate. Other creates in this
//! workspace re-export this as a static or dynamic library.
//!
//! Different constraints are referred to with unique identifiers. Currently,
//! valid values are
//!
//! * `0` for classic Sudoku
//! * `1` for diagonals Sudoku
//! * `2` for knight's move Sudoku
//! * `3` for king's move Sudoku

use crate::sync::CancelHandle;

use serde::{Deserialize, Serialize};

use std::ffi::{CStr, CString};
use std::mem;
use std::os::raw::c_char;
use std::sync::mpsc::{self, Sender};
use std::thread;

use sudoku_variants::{Sudoku, SudokuGrid};
use sudoku_variants::constraint::{
    CompositeConstraint,
    Constraint,
    DefaultConstraint,
    DiagonallyAdjacentConstraint,
    DiagonalsConstraint,
    KnightsMoveConstraint
};
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
    Strategy,
    SudokuInfo,
    TupleStrategy
};

mod sync;

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

struct CancellableStrategy<S> {
    strategy: S,
    handle: CancelHandle
}

impl<S> CancellableStrategy<S> {
    fn new(strategy: S, handle: CancelHandle) -> CancellableStrategy<S> {
        CancellableStrategy {
            strategy,
            handle
        }
    }
}

impl<S: Strategy> Strategy for CancellableStrategy<S> {
    fn apply<C>(&self, sudoku_info: &mut SudokuInfo<C>) -> bool
    where
        C: Constraint + Clone + 'static
    {
        self.handle.assert_not_cancelled();
        self.strategy.apply(sudoku_info)
    }
}

fn difficulty_0(_: CancelHandle) -> impl Solver {
    NotSolver
}

fn difficulty_inf(_: CancelHandle) -> impl Solver {
    DummyPerfectSolver
}

fn default_difficulty_1(_: CancelHandle) -> impl Solver {
    StrategicSolver::new(OnlyCellStrategy)
}

fn default_difficulty_2(handle: CancelHandle) -> impl Solver {
    StrategicSolver::new(
        CancellableStrategy::new(
            CompositeStrategy::new(
                CompositeStrategy::new(
                    OnlyCellStrategy,
                    NakedSingleStrategy
                ),
                BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(0), NoStrategy)
            ), handle
        )
    )
}

fn default_difficulty_3(handle: CancelHandle) -> impl Solver {
    StrategicSolver::new(CancellableStrategy::new(CompositeStrategy::new(
        CompositeStrategy::new(
            OnlyCellStrategy,
            NakedSingleStrategy
        ),
        CompositeStrategy::new(
            BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(1),
                CancellableStrategy::new(
                    BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(0), NoStrategy),
                    handle.clone())),
            TupleStrategy::new(|_| 2)
        )
    ), handle))
}

fn default_difficulty_4(handle: CancelHandle) -> impl Solver {
    StrategicSolver::new(CancellableStrategy::new(
        CompositeStrategy::new(
            CompositeStrategy::new(
                OnlyCellStrategy,
                NakedSingleStrategy
            ),
            CompositeStrategy::new(
                CompositeStrategy::new(
                    BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(1),
                        CancellableStrategy::new(
                            BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(0), NoStrategy),
                            handle.clone())
                        ),
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
        ), handle)
    )
}

fn default_difficulty_5(_: CancelHandle) -> impl Solver {
    StrategicBacktrackingSolver::new(
        CompositeStrategy::new(
            OnlyCellStrategy,
            NakedSingleStrategy
        )
    )
}

type DefaultDiagonalsConstraint =
    CompositeConstraint<DefaultConstraint, DiagonalsConstraint>;
type DefaultKnightsMoveConstraint =
    CompositeConstraint<DefaultConstraint, KnightsMoveConstraint>;
type DefaultKingsMoveConstraint =
    CompositeConstraint<DefaultConstraint, DiagonallyAdjacentConstraint>;

fn default_constraint() -> DefaultConstraint {
    DefaultConstraint
}

fn diagonals_constraint() -> DefaultDiagonalsConstraint {
    CompositeConstraint::new(
        DefaultConstraint,
        DiagonalsConstraint
    )
}

fn knights_move_constraint() -> DefaultKnightsMoveConstraint {
    CompositeConstraint::new(
        DefaultConstraint,
        KnightsMoveConstraint
    )
}

fn kings_move_constraint() -> DefaultKingsMoveConstraint {
    CompositeConstraint::new(
        DefaultConstraint,
        DiagonallyAdjacentConstraint
    )
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
    constraint_cons: FC, cancel_handle: CancelHandle,
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

    while !cancel_handle.is_cancelled() {
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
    FSL: Fn(CancelHandle) -> SL,
    SU: Solver + Send + 'static,
    FSU: Fn(CancelHandle) -> SU,
    SG: Solver + Send + 'static,
    FSG: Fn(CancelHandle) -> SG,
    C: Constraint + Clone + Send + 'static,
    FC: Fn() -> C + Send + Copy + 'static
{
    // TODO replace with thread::available_parallelism once stable
    let threads = num_cpus::get();
    let mut cancel_handles = Vec::new();
    let (sender, receiver) = mpsc::channel();

    for _ in 0..threads {
        let cancel_handle = CancelHandle::new();
        cancel_handles.push(cancel_handle.clone());
        let result_sender = Sender::clone(&sender);
        let lower_difficulty_bound_solver =
            lower_difficulty_bound_solver_cons(cancel_handle.clone());
        let upper_difficulty_bound_solver =
            upper_difficulty_bound_solver_cons(cancel_handle.clone());
        let generator_solver = generator_solver_cons(cancel_handle.clone());
        thread::spawn(move || gen_with_difficulty_thread(
            lower_difficulty_bound_solver,
            upper_difficulty_bound_solver, generator_solver,
            constraint_cons, cancel_handle, result_sender));
    }

    drop(sender);

    let result = receiver.recv().unwrap();

    for cancel_handle in cancel_handles {
        cancel_handle.cancel();
    }

    result
}

fn gen_simple<C, FC>(difficulty: i32, constraint_cons: FC) -> *const c_char
where
    C: Constraint + Clone + Send + Serialize + 'static,
    FC: Fn() -> C + Send + Copy + 'static
{
    let sudoku = match difficulty {
        1 => gen_with_difficulty(
            difficulty_0,
            default_difficulty_1,
            default_difficulty_1,
            constraint_cons),
        2 => gen_with_difficulty(
            default_difficulty_1,
            default_difficulty_2,
            default_difficulty_2,
            constraint_cons),
        3 => gen_with_difficulty(
            default_difficulty_2,
            default_difficulty_3,
            default_difficulty_5,
            constraint_cons),
        4 => gen_with_difficulty(
            default_difficulty_3,
            default_difficulty_4,
            default_difficulty_5,
            constraint_cons),
        5 => gen_with_difficulty(
            default_difficulty_4,
            difficulty_inf,
            default_difficulty_5,
            constraint_cons),
        _ => panic!("Invalid difficulty: {}", difficulty)
    };

    let json = serde_json::to_string(&sudoku).unwrap();
    let json_c = CString::new(json).unwrap();
    let json_ptr = json_c.as_ptr();
    mem::forget(json_c);
    json_ptr
}

/// Generates a 9x9 Sudoku with the provided constraint and difficulty and
/// returns its JSON serialization.
///
/// # Arguments
///
/// * `constraint`: A identifier for the constraint that is used. For valid
/// values, please refer to the crate-level documentation.
/// * `difficulty`: The difficulty of the generated Sudoku on a scale from 1 to
/// 5 (both inclusive).
#[no_mangle]
pub extern fn gen(constraint: i32, difficulty: i32) -> *const c_char {
    match constraint {
        0 => gen_simple(difficulty, default_constraint),
        1 => gen_simple(difficulty, diagonals_constraint),
        2 => gen_simple(difficulty, knights_move_constraint),
        3 => gen_simple(difficulty, kings_move_constraint),
        _ => panic!("Invalid constraint identifier: {}", constraint)
    }
}

fn check_with<C>(json: *const c_char) -> u8
where
    C: Constraint + Clone,
    for<'de> C: Deserialize<'de>
{
    let json = unsafe { CStr::from_ptr(json) }.to_str().unwrap();
    let sudoku: Sudoku<C> = serde_json::from_str(json).unwrap();

    if sudoku.is_valid() { 1 } else { 0 }
}

/// Checks whether all constraints in a 9x9 Sudoku with the given constraint
/// types are satisfied.
///
/// # Arguments
///
/// * `constraint`: A identifier for the constraint that is used. For valid
/// values, please refer to the crate-level documentation.
#[no_mangle]
pub extern fn check(constraint: i32, json: *const c_char) -> u8 {
    match constraint {
        0 => check_with::<DefaultConstraint>(json),
        1 => check_with::<DefaultDiagonalsConstraint>(json),
        2 => check_with::<DefaultKnightsMoveConstraint>(json),
        3 => check_with::<DefaultKingsMoveConstraint>(json),
        _ => panic!("Invalid constraint identifier: {}", constraint)
    }
}

/// Returns 42. For tests that the library was loaded correctly.
#[no_mangle]
pub extern fn test() -> i32 {
    42
}
