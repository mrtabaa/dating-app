export interface UpdateResult{
    isAcknowledged: boolean,
    isModifiedCountAvailable: boolean,
    matchedCount: number,
    modifiedCount: number,
    upsertedId: any
}