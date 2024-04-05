import { OptionalDate } from "../types/optional-date.type";
import { OptionalString } from "../types/optional-string.type";
import { Photo } from "./photo.model";

export interface Member {
    schema: OptionalString,
    userName: OptionalString,
    age: number | undefined,
    knownAs: OptionalString,
    created: OptionalDate,
    lastActive: OptionalDate,
    gender: OptionalString,
    introduction: OptionalString,
    lookingFor: OptionalString,
    interests: OptionalString,
    city: OptionalString,
    country: OptionalString,
    photos: Photo[]
}
