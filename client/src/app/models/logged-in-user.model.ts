export interface LoggedInUser {
    id: string;
    knownAs: string;
    email: string;
    token: string;
    profilePhotoUrl: string;
}