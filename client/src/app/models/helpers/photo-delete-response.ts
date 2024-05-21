export interface PhotoDeleteResponse {
    newMainUrl: string; // It's set only when the main photo is deleted. It will have the next main photo's blobUrl
    successMessage: string;
    isDeletionFail: boolean;
}