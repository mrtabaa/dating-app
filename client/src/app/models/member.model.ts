import { Photo } from "./photo.model";

export interface Member {
    schema: string;
    userName: string;
    age: number;
    dateOfBirth: Date,
    knownAs: string;
    created: Date;
    lastActive: Date;
    gender: string;
    introduction: string;
    lookingFor: string;
    interests: string;
    city: string;
    country: string;
    photos: Photo[];
    isFollowing: boolean;
}
