use crate::constraint::AnyConstraint;
use crate::generate::{self, CancellableStrategy};
use crate::sync::CancelHandle;

use serde::Serialize;

use sudoku_variants::Sudoku;
use sudoku_variants::constraint::{
    CompositeConstraint,
    Constraint,
    DefaultConstraint,
    DiagonallyAdjacentConstraint,
    DiagonalsConstraint,
    KnightsMoveConstraint
};
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
type DefaultChessConstraint =
    CompositeConstraint<DefaultConstraint,
        CompositeConstraint<KnightsMoveConstraint,
            DiagonallyAdjacentConstraint>>;

pub(crate) fn default_constraint() -> DefaultConstraint {
    DefaultConstraint
}

pub(crate) fn diagonals_constraint() -> DefaultDiagonalsConstraint {
    CompositeConstraint::new(
        DefaultConstraint,
        DiagonalsConstraint
    )
}

pub(crate) fn knights_move_constraint() -> DefaultKnightsMoveConstraint {
    CompositeConstraint::new(
        DefaultConstraint,
        KnightsMoveConstraint
    )
}

pub(crate) fn kings_move_constraint() -> DefaultKingsMoveConstraint {
    CompositeConstraint::new(
        DefaultConstraint,
        DiagonallyAdjacentConstraint
    )
}

pub(crate) fn chess_constraint() -> DefaultChessConstraint {
    CompositeConstraint::new(
        DefaultConstraint,
        CompositeConstraint::new(
            KnightsMoveConstraint,
            DiagonallyAdjacentConstraint
        )
    )
}

pub(crate) fn gen_simple<C, FC>(difficulty: i32, constraint_cons: FC) -> Sudoku<C>
where
    C: Constraint + Clone + Into<AnyConstraint> + Send + Serialize + 'static,
    FC: Fn() -> C + Send + Copy + 'static
{
    match difficulty {
        1 => generate::gen_with_difficulty(
            generate::difficulty_0,
            default_difficulty_1,
            default_difficulty_1,
            constraint_cons,
            generate::constraint_identity),
        2 => generate::gen_with_difficulty(
            default_difficulty_1,
            default_difficulty_2,
            default_difficulty_2,
            constraint_cons,
            generate::constraint_identity),
        3 => generate::gen_with_difficulty(
            default_difficulty_2,
            default_difficulty_3,
            default_difficulty_5,
            constraint_cons,
            generate::constraint_identity),
        4 => generate::gen_with_difficulty(
            default_difficulty_3,
            default_difficulty_4,
            default_difficulty_5,
            constraint_cons,
            generate::constraint_identity),
        5 => generate::gen_with_difficulty(
            default_difficulty_4,
            generate::difficulty_inf,
            default_difficulty_5,
            constraint_cons,
            generate::constraint_identity),
        _ => panic!("Invalid difficulty: {}", difficulty)
    }
}
