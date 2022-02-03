use std::sync::{Arc, RwLock};

/// A shared handle that allows signaling that an operation was canceled.
#[derive(Clone)]
pub(crate) struct CancelHandle(Arc<RwLock<bool>>);

impl CancelHandle {

    /// Creates a new cancel handle. Initially, it is not cancelled.
    pub(crate) fn new() -> CancelHandle {
        CancelHandle(Arc::new(RwLock::new(false)))
    }

    /// Indicates whether the operation associated with this handle is
    /// cancelled.
    pub(crate) fn is_cancelled(&self) -> bool {
        *self.0.read().unwrap()
    }

    /// Panics if the operation associated with this handle is cancelled.
    pub(crate) fn assert_not_cancelled(&self) {
        if self.is_cancelled() {
            panic!("cancelled")
        }
    }

    /// Cancels the operation associated with this handle.
    pub(crate) fn cancel(&self) {
        *self.0.write().unwrap() = true;
    }
}
