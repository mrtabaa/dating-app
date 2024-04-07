import { OptionalString } from "../types/optional-string.type";

export interface UserUpdate {
    userName: string,
    introduction: string,
    lookingFor: string,
    interests: string,
    city: string,
    country: string
}