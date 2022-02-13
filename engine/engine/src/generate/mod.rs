use crate::sync::CancelHandle;

use std::sync::mpsc::{self, Sender};
use std::thread;

use sudoku_variants::{Sudoku, SudokuGrid};
use sudoku_variants::constraint::Constraint;
use sudoku_variants::generator::{Generator, Reducer};
use sudoku_variants::solver::{Solution, Solver};
use sudoku_variants::solver::strategy::{Strategy, SudokuInfo};

mod sandwich;
mod simple;

pub(crate) use sandwich::*;
pub(crate) use simple::*;

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

fn can_solve<C, S>(sudoku: &Sudoku<C>, solver: &S) -> bool
where
    C: Constraint + Clone + 'static,
    S: Solver
{
    matches!(solver.solve(sudoku), Solution::Unique(_))
}

fn constraint_identity<C>(constraint: C, _: &SudokuGrid) -> C {
    constraint
}

fn gen_with_difficulty_thread<SL, SU, SG, C1, C2, FC1, FC2>(
    lower_difficulty_bound_solver: SL,
    upper_difficulty_bound_solver: SU, generator_solver: SG,
    constraint_cons: FC1, constraint_transform: FC2,
    cancel_handle: CancelHandle, result_sender: Sender<Sudoku<C2>>)
where
    SL: Solver,
    SU: Solver,
    SG: Solver,
    C1: Constraint + Clone + 'static,
    C2: Constraint + Clone + 'static,
    FC1: Fn() -> C1,
    FC2: Fn(C1, &SudokuGrid) -> C2
{
    let mut generator = Generator::new_default();
    let mut reducer = Reducer::new(generator_solver, rand::thread_rng());

    while !cancel_handle.is_cancelled() {
        let constraint = constraint_cons();
        let sudoku = generator.generate(3, 3, constraint).unwrap();
        let (grid, constraint) = sudoku.into_raw_parts();
        let constraint = constraint_transform(constraint, &grid);
        let mut sudoku = Sudoku::new_with_grid(grid, constraint);
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
/// * `constraint_cons`: A closure that creates the initial constraint of the
/// generated Sudoku. This is the constraint used for generation.
/// * `constraint_transform`: A closure that takes the initial constraint
/// produced by `constraint_cons` and the full Sudoku grid and returns the
/// constraint of the Sudoku to reduce. For stateless constraints, this is the
/// identity, but reducible constraints may compute things like sandwich sums
/// or generate random Killer Sudoku cages here.
fn gen_with_difficulty<SL, FSL, SU, FSU, SG, FSG, C1, C2, FC1, FC2>(
    lower_difficulty_bound_solver_cons: FSL,
    upper_difficulty_bound_solver_cons: FSU, generator_solver_cons: FSG,
    constraint_cons: FC1, constraint_transform: FC2) -> Sudoku<C2>
where
    SL: Solver + Send + 'static,
    FSL: Fn(CancelHandle) -> SL,
    SU: Solver + Send + 'static,
    FSU: Fn(CancelHandle) -> SU,
    SG: Solver + Send + 'static,
    FSG: Fn(CancelHandle) -> SG,
    C1: Constraint + Clone + 'static,
    C2: Constraint + Clone + Send + 'static,
    FC1: Fn() -> C1 + Send + Copy + 'static,
    FC2: Fn(C1, &SudokuGrid) -> C2 + Send + Copy + 'static
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
            lower_difficulty_bound_solver, upper_difficulty_bound_solver,
            generator_solver, constraint_cons, constraint_transform,
            cancel_handle, result_sender));
    }

    drop(sender);

    let result = receiver.recv().unwrap();

    for cancel_handle in cancel_handles {
        cancel_handle.cancel();
    }

    result
}
