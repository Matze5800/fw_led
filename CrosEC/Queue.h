EXTERN_C_START

typedef struct _QUEUE_CONTEXT {
	char empty;
} QUEUE_CONTEXT, *PQUEUE_CONTEXT;

WDF_DECLARE_CONTEXT_TYPE_WITH_NAME(QUEUE_CONTEXT, QueueGetContext)

NTSTATUS CrosECQueueInitialize(_In_ WDFDEVICE Device);

// Events
EVT_WDF_IO_QUEUE_IO_DEVICE_CONTROL CrosECEvtIoDeviceControl;
EVT_WDF_IO_QUEUE_IO_STOP CrosECEvtIoStop;

EXTERN_C_END
