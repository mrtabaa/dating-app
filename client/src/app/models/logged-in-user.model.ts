export interface LoggedInUser {
    hashedId: string;
    username: string;
    knownAs: string;
    token: string;
    gender: string
    profilePhotoUrl: string;
}