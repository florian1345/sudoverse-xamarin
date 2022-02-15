use crate::generate::{self, CancellableStrategy};
use crate::sync::CancelHandle;

use sudoku_variants::{Sudoku, SudokuGrid};
use sudoku_variants::constraint::{
    CompositeConstraint,
    CompositeData,
    DefaultConstraint,
    SandwichConstraint,
};
use sudoku_variants::constraint::sandwich::SandwichReduction;
use sudoku_variants::generator::Reduction;
use sudoku_variants::solver::Solver;
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
use sudoku_variants::solver::strategy::specific::{
    SandwichBunPlacementStrategy,
    SandwichPossibilitiesStrategy
};

pub(crate) type SandwichStrategy = CompositeStrategy<
    SandwichBunPlacementStrategy, SandwichPossibilitiesStrategy>;

pub(crate) fn sandwich_strategy() -> SandwichStrategy {
    CompositeStrategy::new(
        SandwichBunPlacementStrategy,
        SandwichPossibilitiesStrategy
    )
}

fn sandwich_difficulty_1(handle: CancelHandle) -> impl Solver {
    StrategicSolver::new(
        CancellableStrategy::new(
            CompositeStrategy::new(
                OnlyCellStrategy,
                sandwich_strategy()
            ), handle
        )
    )
}

fn sandwich_difficulty_2(handle: CancelHandle) -> impl Solver {
    StrategicSolver::new(
        CancellableStrategy::new(
            CompositeStrategy::new(
                CompositeStrategy::new(
                    OnlyCellStrategy,
                    NakedSingleStrategy
                ),
                CompositeStrategy::new(
                    BoundedCellsBacktrackingStrategy::new(|_| 2, |_| Some(0), NoStrategy),
                    sandwich_strategy()
                )
            ), handle
        )
    )
}

fn sandwich_difficulty_3(handle: CancelHandle) -> impl Solver {
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
            CompositeStrategy::new(
                TupleStrategy::new(|_| 2),
                sandwich_strategy()
            )
        )
    ), handle))
}

fn sandwich_difficulty_4(handle: CancelHandle) -> impl Solver {
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
                    CompositeStrategy::new(
                        BoundedOptionsBacktrackingStrategy::new(|_| 2, |_| Some(2),
                            CompositeStrategy::new(
                                OnlyCellStrategy,
                                NakedSingleStrategy
                            )
                        ),
                        sandwich_strategy()
                    )
                )
            )
        ), handle)
    )
}

fn sandwich_difficulty_5(handle: CancelHandle) -> impl Solver {
    StrategicBacktrackingSolver::new(
        CancellableStrategy::new(
            CompositeStrategy::new(
                CompositeStrategy::new(
                    OnlyCellStrategy,
                    NakedSingleStrategy
                ),
                sandwich_strategy()
            ),
            handle
        )
    )
}

type DefaultSandwichConstraint =
    CompositeConstraint<DefaultConstraint, SandwichConstraint>;

fn make_sandwich_constraint(c1: DefaultConstraint, grid: &SudokuGrid)
        -> DefaultSandwichConstraint {
    let c2 = SandwichConstraint::new_full(grid);
    CompositeConstraint::new(c1, c2)
}

fn prioritize(reduction: &Reduction<CompositeData<(), SandwichReduction>>)
        -> f64 {
    match reduction {
        Reduction::RemoveDigit { .. } => 0.0,
        Reduction::ReduceConstraint { .. } => 100.0
    }
}

pub(crate) fn gen_sandwich(difficulty: i32)
        -> Sudoku<DefaultSandwichConstraint> {
    match difficulty {
        1 => generate::gen_with_difficulty(
            generate::difficulty_0,
            sandwich_difficulty_1,
            sandwich_difficulty_1,
            generate::default_constraint,
            make_sandwich_constraint,
            prioritize),
        2 => generate::gen_with_difficulty(
            sandwich_difficulty_1,
            sandwich_difficulty_2,
            sandwich_difficulty_2,
            generate::default_constraint,
            make_sandwich_constraint,
            prioritize),
        3 => generate::gen_with_difficulty(
            sandwich_difficulty_2,
            sandwich_difficulty_3,
            sandwich_difficulty_5,
            generate::default_constraint,
            make_sandwich_constraint,
            prioritize),
        4 => generate::gen_with_difficulty(
            sandwich_difficulty_3,
            sandwich_difficulty_4,
            sandwich_difficulty_5,
            generate::default_constraint,
            make_sandwich_constraint,
            prioritize),
        5 => generate::gen_with_difficulty(
            sandwich_difficulty_4,
            generate::difficulty_inf,
            sandwich_difficulty_5,
            generate::default_constraint,
            make_sandwich_constraint,
            prioritize),
        _ => panic!("Invalid difficulty: {}", difficulty)
    }
}
