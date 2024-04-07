import { OptionalDate } from "../types/optional-date.type";
import { OptionalString } from "../types/optional-string.type";
import { Photo } from "./photo.model";

export interface Member {
    schema: string;
    userName: string;
    age: number;
    knownAs: string;
    created: Date;
    lastActive: Date;
    gender: string;
    introduction: string;
    lookingFor: string;
    interests: string;
    city: string;
    country: string;
    photos: Photo[]
}
