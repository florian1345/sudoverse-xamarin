use serde::Serialize;

use sudoku_variants::Sudoku;
use sudoku_variants::constraint::Constraint;

/// The response to a `check` call to the engine. To be serialized for return.
#[derive(Serialize)]
#[serde(tag = "type", content = "value")]
pub(crate) enum CheckResponse {

    /// Indicates that the Sudoku is valid.
    #[serde(rename = "valid")]
    Valid,

    /// Indicates that the Sudoku is invalid. A vector of coordinates (column
    /// and row) is provided.
    #[serde(rename = "invalid")]
    Invalid(Vec<(usize, usize)>)
}

impl CheckResponse {

    /// Determines the appropriate check response for the given Sudoku, i.e.
    /// [CheckResponse::Valid] if it is valid and [CheckResponse::Invalid] with
    /// all invalid cells otherwise.
    pub(crate) fn from_sudoku<C>(sudoku: &Sudoku<C>) -> CheckResponse
    where
        C: Constraint + Clone
    {
        if sudoku.is_valid() {
            CheckResponse::Valid
        }
        else {
            let size = sudoku.grid().size();
            let mut errors = Vec::new();

            for row in 0..size {
                for column in 0..size {
                    if !sudoku.is_valid_cell(column, row).unwrap() {
                        errors.push((column, row));
                    }
                }
            }

            CheckResponse::Invalid(errors)
        }
    }
}
