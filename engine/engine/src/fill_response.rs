use serde::Serialize;

use sudoku_variants::{Sudoku, SudokuGrid};
use sudoku_variants::constraint::Constraint;
use sudoku_variants::generator::Generator;

/// The response to a `check` call to the engine. To be serialized for return.
#[derive(Serialize)]
#[serde(tag = "type", content = "value")]
pub(crate) enum FillResponse {

    /// Indicates that the Sudoku is valid.
    #[serde(rename = "ok")]
    Ok(SudokuGrid),

    /// Indicates that the Sudoku is invalid. A vector of coordinates (column
    /// and row) is provided.
    #[serde(rename = "unsatisfiable")]
    Unsatisfiable
}

impl FillResponse {

    /// Generates an appropriate fill response for the given Sudoku, i.e.
    /// [FillResponse::Ok] with some random [SudokuGrid] that fills the given
    /// Sudoku, or [FillResponse::Unsatisfiable] if there is no such grid.
    pub(crate) fn from_sudoku<C>(mut sudoku: Sudoku<C>) -> FillResponse
    where
        C: Constraint + Clone
    {
        let mut generator = Generator::new_default();

        if generator.fill(&mut sudoku).is_ok() {
            FillResponse::Ok(sudoku.into_raw_parts().0)
        }
        else {
            FillResponse::Unsatisfiable
        }
    }
}
