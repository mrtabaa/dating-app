export interface LoggedInUser {
    hashedId: string;
    email: string;
    knownAs: string;
    token: string;
    gender: string
    profilePhotoUrl: string;
}