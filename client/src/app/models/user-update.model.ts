import { OptionalString } from "../types/optional-string.type";

export interface UserUpdate {
    username: OptionalString,
    introduction: string,
    lookingFor: string,
    interests: string,
    city: string,
    country: string
}