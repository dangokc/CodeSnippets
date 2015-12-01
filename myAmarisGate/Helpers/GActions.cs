
namespace AmarisGate.Helpers
{
    public enum GActions
    {
        // Automated Request made by DNA = 0
        AutomatedRequest,
        // User Requests = 1
        UserReq,
        // Manager requests for own employee = 2
        ManagerReq,
        // Request Cancelled = 3
        Cancelled,
        // Manager Validates Request = 4
        ManagerValidate,
        // Detected In Stock = 5
        DetectedStock,
        // Not Detected In Stock = 6
        NotDetectedStock,
        // Director Validates Request with Non Standard Product = 7
        DirectorValidate,
        // Delivered = 8
        Delivered,
        // Sent For Order = 9
        SentForOrder,
        // Need For Set Up Detected = 10
        NeedSetUpDetected,
        // No Need For Set Up Detected = 11
        NoNeedSetUpDetected,
        // Order Dispatched = 12
        Dispatched,
        // Set Up Completed = 13
        SetUpComplete,
        // Manager Refuses Offer = 14
        ManagerRefuses,
        // HelpDesk Proposes Product = 15
        ExpertProposesProd,
        // Purchasing Dpt. Accepts Products = 16
        ManagerAccepts,
        // Material Set Up = 17
        MaterialSetUp,
        // Material Dispatch = 18
        MaterialDispatch,
        // User Confirms Reception. Request Complete = 19
        UserConfirmsReception,
        // Director Cancels. Price higher than expected = 20
        DirectorCancelPrice,
        // Manager Cancels. Price higher than expected = 21
        ManagerCancelPrice,
        // Purchasing dpt Cancels. Price higher than expected = 22
        PurchasingDptCancelPrice,
        // Director Cancels. Request not relevant or necessary = 23
        DirectorCancelRelevancy,
        // Manager Cancels. Request not relevant or necessary = 24
        ManagerCancelRelevancy,
        // Purchasing dpt Cancels. Request not relevant or necessary = 25
        PurchasingDptCancelRelevancy,
    }
}