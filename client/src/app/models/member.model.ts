import { Photo } from "./photo.model";

export interface Member {
    schema: string,
    id: string,
    name: string,
    email: string,
    age: number,
    knownAs: string,
    created: Date,
    lastActive: Date,
    gender: string,
    introduction: string,
    lookingFor: string,
    interests: string,
    city: string,
    country: string,
    photos: Photo[]
}
