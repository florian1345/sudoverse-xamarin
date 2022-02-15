use crate::constraint::AnyConstraint;
use crate::generate::{self, SandwichStrategy};

use sudoku_variants::{Sudoku, SudokuGrid};
use sudoku_variants::constraint::{
    CompositeConstraint,
    Constraint,
    DefaultConstraint,
    SandwichConstraint,
    Subconstraint
};
use sudoku_variants::solver::{Solution, Solver};
use sudoku_variants::solver::strategy::{
    CompositeStrategy,
    NakedSingleStrategy,
    OnlyCellStrategy,
    StrategicBacktrackingSolver,
    Strategy,
    SudokuInfo
};

type FastestStrategy =
    CompositeStrategy<OnlyCellStrategy, NakedSingleStrategy>;

fn fastest_strategy() -> FastestStrategy {
    CompositeStrategy::new(
        OnlyCellStrategy,
        NakedSingleStrategy
    )
}

enum AnyStrategy {
    Fastest(FastestStrategy),
    Sandwich(SandwichStrategy),
    Composite(Vec<AnyStrategy>)
}

impl Strategy for AnyStrategy {
    fn apply<C>(&self, sudoku_info: &mut SudokuInfo<C>) -> bool
    where
        C: Constraint + Clone + 'static
    {
        match self {
            AnyStrategy::Fastest(s) => s.apply(sudoku_info),
            AnyStrategy::Sandwich(s) => s.apply(sudoku_info),
            AnyStrategy::Composite(ss) => ss.iter().any(|s| s.apply(sudoku_info))
        }
    }
}

fn optimal_strategy(constraint: &AnyConstraint) -> AnyStrategy {
    let mut strategies = vec![AnyStrategy::Fastest(fastest_strategy())];

    if constraint.has_subconstraint::<SandwichConstraint>() {
        strategies.push(AnyStrategy::Sandwich(generate::sandwich_strategy()));
    }

    if strategies.len() == 1 {
        strategies.into_iter().next().unwrap()
    }
    else {
        AnyStrategy::Composite(strategies)
    }
}

fn solve_with<C, S>(sudoku: Sudoku<C>, strategy: S) -> Solution
where
    C: Constraint + Clone + 'static,
    S: Strategy
{
    let solver = StrategicBacktrackingSolver::new(strategy);
    solver.solve(&sudoku)
}

fn solve_with_fastest<C>(grid: SudokuGrid, constraint: C) -> Solution
where
    C: Constraint + Clone + 'static
{
    let sudoku = Sudoku::new_with_grid(grid, constraint);
    solve_with(sudoku, fastest_strategy())
}

fn solve_with_optimal(grid: SudokuGrid, constraint: AnyConstraint)
        -> Solution {
    let strategy = optimal_strategy(&constraint);
    let sudoku = Sudoku::new_with_grid(grid, constraint);
    solve_with(sudoku, strategy)
}

/// Solves the given `sudoku` using an appropriate solver, and returns the
/// solution. Common Sudoku types are treated with specific, statically-typed
/// instances of solution algorithms to improve performance. However, this
/// method works with any [AnyConstraint], it may just be a bit slower than
/// optimal.
pub(crate) fn solve(sudoku: Sudoku<AnyConstraint>) -> Solution {
    let (grid, constraint) = sudoku.into_raw_parts();

    match constraint {
        AnyConstraint::Default =>
            solve_with_fastest(grid, generate::default_constraint()),
        AnyConstraint::Composite(cs) => {
            match cs.as_slice() {
                [AnyConstraint::Default, AnyConstraint::Diagonals] =>
                    solve_with_fastest(grid, generate::diagonals_constraint()),
                [AnyConstraint::Default, AnyConstraint::KnightsMove] => 
                    solve_with_fastest(grid,
                        generate::knights_move_constraint()),
                [AnyConstraint::Default, AnyConstraint::KingsMove] => 
                    solve_with_fastest(grid,
                        generate::kings_move_constraint()),
                [AnyConstraint::Default, AnyConstraint::KnightsMove,
                        AnyConstraint::KingsMove] |
                [AnyConstraint::Default, AnyConstraint::KingsMove,
                        AnyConstraint::KnightsMove] => 
                    solve_with_fastest(grid, generate::chess_constraint()),
                [AnyConstraint::Default, AnyConstraint::Sandwich(sc)] => {
                    let constraint =
                        CompositeConstraint::new(DefaultConstraint, sc.clone());
                    let sudoku = Sudoku::new_with_grid(grid, constraint);
                    let strategy = CompositeStrategy::new(
                        fastest_strategy(), generate::sandwich_strategy());
                    solve_with(sudoku, strategy)
                }
                _ => {
                    let constraint = AnyConstraint::Composite(cs);
                    solve_with_optimal(grid, constraint)
                }
            }
        },
        constraint => solve_with_optimal(grid, constraint)
    }
}
