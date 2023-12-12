import { Photo } from "./photo.model";

export interface User {
    schema: string | undefined,
    id: string | undefined,
    email: string | undefined,
    age: number | undefined,
    knownAs: string | undefined,
    created: Date | undefined,
    lastActive: Date | undefined,
    gender: string | undefined,
    introduction: string | undefined,
    lookingFor: string | undefined,
    interests: string | undefined,
    city: string | undefined,
    country: string | undefined,
    photos: Photo[]
}
