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
//! * `4` for Chess Sudoku (knight's move + king's move)
//! * `5` for sandwich Sudoku

use crate::check_response::CheckResponse;
use crate::constraint::AnyConstraint;
use crate::fill_response::FillResponse;

use serde::{Deserialize, Serialize};

use std::ffi::{CStr, CString};
use std::mem;
use std::os::raw::c_char;

use sudoku_variants::Sudoku;
use sudoku_variants::constraint::Constraint;
use sudoku_variants::solver::Solution;

mod check_response;
mod constraint;
mod fill_response;
mod generate;
mod solve;
mod sync;

fn to_ffi_json(s: &impl Serialize) -> *const c_char {
    let json = serde_json::to_string(s).unwrap();
    let json_c = CString::new(json).unwrap();
    let json_ptr = json_c.as_ptr();
    mem::forget(json_c);
    json_ptr
}

fn sudoku_to_ffi_json<C>(sudoku: Sudoku<C>) -> *const c_char
where
    C: Clone + Constraint + Into<AnyConstraint>
{
    let (grid, constraint) = sudoku.into_raw_parts();
    let constraint: AnyConstraint = constraint.into();
    let sudoku = Sudoku::new_with_grid(grid, constraint);

    to_ffi_json(&sudoku)
}

fn from_ffi_json<T>(json: *const c_char) -> T
where
    for<'de> T: Deserialize<'de>
{
    let json = unsafe { CStr::from_ptr(json) }.to_str().unwrap();
    serde_json::from_str(json).unwrap()
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
        0 => sudoku_to_ffi_json(generate::gen_simple(
            difficulty, 
            generate::default_constraint
        )),
        1 => sudoku_to_ffi_json(generate::gen_simple(
            difficulty, 
            generate::diagonals_constraint
        )),
        2 => sudoku_to_ffi_json(generate::gen_simple(
            difficulty, 
            generate::knights_move_constraint
        )),
        3 => sudoku_to_ffi_json(generate::gen_simple(
            difficulty, 
            generate::kings_move_constraint
        )),
        4 => sudoku_to_ffi_json(generate::gen_simple(
            difficulty, 
            generate::chess_constraint
        )),
        5 => sudoku_to_ffi_json(generate::gen_sandwich(difficulty)),
        _ => panic!("Invalid constraint identifier: {}", constraint)
    }
}

/// Checks whether all constraints in the given Sudoku are satisfied. Returns
/// a [CheckResponse] in JSON form.
///
/// # Arguments
///
/// * `json`: The JSON code of the Sudoku to check.
#[no_mangle]
pub extern fn check(json: *const c_char) -> *const c_char {
    let sudoku: Sudoku<AnyConstraint> = from_ffi_json(json);
    to_ffi_json(&CheckResponse::from_sudoku(&sudoku))
}

/// Generates a random Sudoku grid that satisfies the given Sudoku's constraint
/// while keeping all digits in it. Returns an error if it is unsatisfiable.
/// The result is provided as a [FillResponse] in JSON form.
///
/// # Arguments
///
/// * `json`: The JSON code of the Sudoku to fill.
#[no_mangle]
pub extern fn fill(json: *const c_char) -> *const c_char {
    let sudoku: Sudoku<AnyConstraint> = from_ffi_json(json);
    to_ffi_json(&FillResponse::from_sudoku(sudoku))
}

/// Determines whether the given Sudoku is uniquely solvable. Returns 0 if it
/// is, 1 if it is impossible (i.e. there exist no valid solutions), and 2 if
/// it is ambiguous (i.e. there exist more than one valid solutions).
///
/// # Arguments
///
/// * `json`: The JSON code of the Sudoku to check for solvability.
#[no_mangle]
pub extern fn is_solvable(json: *const c_char) -> u8 {
    let sudoku: Sudoku<AnyConstraint> = from_ffi_json(json);
    let solution = solve::solve(sudoku);

    match solution {
        Solution::Unique(_) => 0,
        Solution::Impossible => 1,
        Solution::Ambiguous => 2
    }
}

/// Returns 42. For tests that the library was loaded correctly.
#[no_mangle]
pub extern fn test() -> i32 {
    42
}
