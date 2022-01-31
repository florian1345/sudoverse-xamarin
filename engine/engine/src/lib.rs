//! This is the common engine crate. It defines all functionality the engine
//! libraries must fulfill as a standard rust crate. Other creates in this
//! workspace re-export this as a static or dynamic library.

use std::ffi::{CStr, CString};
use std::mem;
use std::os::raw::c_char;

use sudoku_variants::Sudoku;
use sudoku_variants::constraint::DefaultConstraint;
use sudoku_variants::generator::{Generator, Reducer};
use sudoku_variants::solver::strategy::{
    CompositeStrategy,
    NakedSingleStrategy,
    OnlyCellStrategy,
    StrategicBacktrackingSolver
};

/// Generates a 9x9 Sudoku with a default constraint and returns its CBOR
/// serialization.
#[no_mangle]
pub extern fn gen_default() -> *const c_char {
    let mut generator = Generator::new_default();
    let mut sudoku = generator.generate(3, 3, DefaultConstraint).unwrap();
    let solver = StrategicBacktrackingSolver::new(
        CompositeStrategy::new(
            NakedSingleStrategy,
            OnlyCellStrategy
        )
    );
    let mut reducer = Reducer::new(solver, rand::thread_rng());
    reducer.reduce(&mut sudoku);
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
