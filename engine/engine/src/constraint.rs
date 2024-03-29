use serde::{Deserialize, Serialize};

use sudoku_variants::SudokuGrid;
use sudoku_variants::constraint::{
    CompositeConstraint,
    Constraint,
    DefaultConstraint,
    DiagonallyAdjacentConstraint,
    DiagonalsConstraint,
    Group,
    KnightsMoveConstraint,
    ReductionError,
    SandwichConstraint
};
use sudoku_variants::constraint::sandwich::SandwichReduction;

/// An enumeration that multiplexes all different reduction types of
/// constraints that are used in the app. This is the reduction type of the
/// [AnyConstraint] type (see [Constraint::Reduction]).
pub(crate) enum AnyReduction {

    /// A reduction of the [AnyConstraint::Sandwich] variant.
    Sandwich(SandwichReduction),

    /// A reduction of the [AnyConstraint::Composite] variant. 
    Composite {

        /// The index of the constraint within the composite constraint to
        /// which this reduction applies.
        index: usize,

        /// The reduction to be applied to the individual constraint.
        reduction: Box<AnyReduction>
    }
}

/// An enumeration that multiplexes all different revert info types of
/// constraints that are used in the app. This is the revert info type of the
/// [AnyConstraint] type (see [Constraint::RevertInfo]).
pub(crate) enum AnyRevertInfo {

    /// A revert info of the [AnyConstraint::Sandwich] variant.
    Sandwich(usize),

    /// A revert info of the [AnyConstraint::Composite] variant. 
    Composite {

        /// The index of the constraint within the composite constraint to
        /// which this revert info applies.
        index: usize,

        /// The revert info for the individual constraint.
        revert_info: Box<AnyRevertInfo>
    }
}

/// An enumeration of the different constraints that are used in the app. This
/// allows monomorphic Sudoku in contexts where the constraint may not be clear
/// statically, however it comes at a performance cost. This is mainly to be
/// used in cases where performance is not a big issue and for serialization
/// and deserialization.
///
/// It can be obtained from all supported specific constraint types via the
/// [From] trait.
#[derive(Clone, Deserialize, Serialize)]
#[serde(tag = "type", content = "value")]
pub(crate) enum AnyConstraint {

    /// Represents a [DefaultConstraint].
    #[serde(rename = "default")]
    Default,

    /// Represents a [DiagonalsConstraint].
    #[serde(rename = "diagonals")]
    Diagonals,

    /// Represents a [KnightsMoveConstraint].
    #[serde(rename = "knights-move")]
    KnightsMove,

    /// Represents a [DiagonallyAdjacentConstraint].
    #[serde(rename = "kings-move")]
    KingsMove,

    /// Wraps a [SandwichConstraint].
    #[serde(rename = "sandwich")]
    Sandwich(SandwichConstraint),

    /// A composite constraint consisting of the wrapped [AnyConstraint]s. This
    /// is different to the [CompositeConstraint] of the `sudoku-variants`
    /// crate in that the element types are not known statically and it can
    /// thus have variable size.
    #[serde(rename = "composite")]
    Composite(Vec<AnyConstraint>)
}

impl Constraint for AnyConstraint {

    type Reduction = AnyReduction;
    type RevertInfo = AnyRevertInfo;

    fn check(&self, grid: &SudokuGrid) -> bool {
        match self {
            AnyConstraint::Default => DefaultConstraint.check(grid),
            AnyConstraint::Diagonals => DiagonalsConstraint.check(grid),
            AnyConstraint::KnightsMove => KnightsMoveConstraint.check(grid),
            AnyConstraint::KingsMove =>
                DiagonallyAdjacentConstraint.check(grid),
            AnyConstraint::Sandwich(c) => c.check(grid),
            AnyConstraint::Composite(cs) =>
                cs.iter().all(|c| c.check(grid))
        }
    }

    fn check_cell(&self, grid: &SudokuGrid, column: usize, row: usize)
            -> bool {
        match self {
            AnyConstraint::Default =>
                DefaultConstraint.check_cell(grid, column, row),
            AnyConstraint::Diagonals =>
                DiagonalsConstraint.check_cell(grid, column, row),
            AnyConstraint::KnightsMove =>
                KnightsMoveConstraint.check_cell(grid, column, row),
            AnyConstraint::KingsMove =>
                DiagonallyAdjacentConstraint.check_cell(grid, column, row),
            AnyConstraint::Sandwich(c) => c.check_cell(grid, column, row),
            AnyConstraint::Composite(cs) =>
                cs.iter().all(|c| c.check_cell(grid, column, row))
        }
    }

    fn check_number(&self, grid: &SudokuGrid, column: usize, row: usize,
            number: usize) -> bool {
        match self {
            AnyConstraint::Default =>
                DefaultConstraint.check_number(grid, column, row, number),
            AnyConstraint::Diagonals =>
                DiagonalsConstraint.check_number(grid, column, row, number),
            AnyConstraint::KnightsMove =>
                KnightsMoveConstraint.check_number(grid, column, row, number),
            AnyConstraint::KingsMove =>
                DiagonallyAdjacentConstraint.check_number(
                    grid, column, row, number),
            AnyConstraint::Sandwich(c) =>
                c.check_number(grid, column, row, number),
            AnyConstraint::Composite(cs) =>
                cs.iter().all(|c| c.check_number(grid, column, row, number))
        }
    }

    fn get_groups(&self, grid: &SudokuGrid) -> Vec<Group> {
        match self {
            AnyConstraint::Default => DefaultConstraint.get_groups(grid),
            AnyConstraint::Diagonals => DiagonalsConstraint.get_groups(grid),
            AnyConstraint::KnightsMove =>
                KnightsMoveConstraint.get_groups(grid),
            AnyConstraint::KingsMove =>
                DiagonallyAdjacentConstraint.get_groups(grid),
            AnyConstraint::Sandwich(c) => c.get_groups(grid),
            AnyConstraint::Composite(cs) =>
                cs.iter()
                    .flat_map(|c| c.get_groups(grid).into_iter())
                    .collect()
        }
    }

    fn list_reductions(&self, solution: &SudokuGrid) -> Vec<AnyReduction> {
        match self {
            AnyConstraint::Default => vec![],
            AnyConstraint::Diagonals => vec![],
            AnyConstraint::KnightsMove => vec![],
            AnyConstraint::KingsMove => vec![],
            AnyConstraint::Sandwich(c) =>
                c.list_reductions(solution).into_iter()
                    .map(|r| AnyReduction::Sandwich(r))
                    .collect(),
            AnyConstraint::Composite(cs) =>
                cs.iter().enumerate()
                    .flat_map(|(i, c)| c.list_reductions(solution).into_iter()
                        .map(move |r| AnyReduction::Composite {
                            index: i,
                            reduction: Box::new(r)
                        }))
                    .collect()
        }
    }

    fn reduce(&mut self, solution: &SudokuGrid, reduction: &AnyReduction)
            -> Result<AnyRevertInfo, ReductionError> {
        match self {
            AnyConstraint::Default => Err(ReductionError::InvalidReduction),
            AnyConstraint::Diagonals => Err(ReductionError::InvalidReduction),
            AnyConstraint::KnightsMove => Err(ReductionError::InvalidReduction),
            AnyConstraint::KingsMove => Err(ReductionError::InvalidReduction),
            AnyConstraint::Sandwich(c) =>
                match reduction {
                    AnyReduction::Sandwich(reduction) =>
                        Ok(AnyRevertInfo::Sandwich(
                            c.reduce(solution, reduction)?)),
                    _ => panic!("invalid reduce")
                }
            AnyConstraint::Composite(cs) => {
                match reduction {
                    AnyReduction::Composite { index, reduction } => {
                        let constraint = cs.get_mut(*index)
                            .ok_or(ReductionError::InvalidReduction)?;
                        let revert_info =
                            constraint.reduce(solution, reduction)?;
                        let revert_info = AnyRevertInfo::Composite {
                            index: *index,
                            revert_info: Box::new(revert_info)
                        };
                        Ok(revert_info)
                    }
                    _ => panic!("invalid reduce")
                }
            }
        }
    }

    fn revert(&mut self, solution: &SudokuGrid, reduction: &AnyReduction,
            revert_info: AnyRevertInfo) {
        match self {
            AnyConstraint::Default => panic!("invalid revert"),
            AnyConstraint::Diagonals => panic!("invalid revert"),
            AnyConstraint::KnightsMove => panic!("invalid revert"),
            AnyConstraint::KingsMove => panic!("invalid revert"),
            AnyConstraint::Sandwich(c) =>
                match (reduction, revert_info) {
                    (
                        AnyReduction::Sandwich(reduction),
                        AnyRevertInfo::Sandwich(revert_info)
                    ) => c.revert(solution, reduction, revert_info),
                    _ => panic!("invalid revert")
                }
            AnyConstraint::Composite(cs) => {
                match (reduction, revert_info) {
                    (AnyReduction::Composite {
                        index: red_index,
                        reduction
                    }, AnyRevertInfo::Composite {
                        index: rev_info_index,
                        revert_info
                    }) => {
                        if *red_index != rev_info_index {
                            panic!("invalid revert");
                        }

                        let index = rev_info_index;
                        let constraint = cs.get_mut(index)
                            .unwrap_or_else(|| panic!("invalid revert"));
                        constraint.revert(solution, reduction, *revert_info);
                    },
                    _ => panic!("invalid revert")
                }
            }
        }
    }
}

impl From<DefaultConstraint> for AnyConstraint {
    fn from(_: DefaultConstraint) -> AnyConstraint {
        AnyConstraint::Default
    }
}

impl From<DiagonalsConstraint> for AnyConstraint {
    fn from(_: DiagonalsConstraint) -> AnyConstraint {
        AnyConstraint::Diagonals
    }
}

impl From<KnightsMoveConstraint> for AnyConstraint {
    fn from(_: KnightsMoveConstraint) -> AnyConstraint {
        AnyConstraint::KnightsMove
    }
}

impl From<DiagonallyAdjacentConstraint> for AnyConstraint {
    fn from(_: DiagonallyAdjacentConstraint) -> AnyConstraint {
        AnyConstraint::KingsMove
    }
}

impl From<SandwichConstraint> for AnyConstraint {
    fn from(c: SandwichConstraint) -> AnyConstraint {
        AnyConstraint::Sandwich(c)
    }
}

fn add_any_constraints<C>(constraintss: &mut Vec<AnyConstraint>, c: C)
where
    C: Into<AnyConstraint>
{
    match c.into() {
        AnyConstraint::Composite(mut cs) => constraintss.append(&mut cs),
        c => constraintss.push(c)
    }
}

impl<C1, C2> From<CompositeConstraint<C1, C2>> for AnyConstraint
where
    C1: Constraint + Clone + Into<AnyConstraint>,
    C2: Constraint + Clone + Into<AnyConstraint>
{
    fn from(constraint: CompositeConstraint<C1, C2>) -> AnyConstraint {
        let (c1, c2) = constraint.into_components();
        let mut constraints = Vec::new();

        add_any_constraints(&mut constraints, c1);
        add_any_constraints(&mut constraints, c2);

        AnyConstraint::Composite(constraints)
    }
}
