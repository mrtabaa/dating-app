export interface LoggedInUser {
    userName: string;
    knownAs: string;
    token: string;
    gender: string
    profilePhotoUrl: string;
    roles: string[];
    isProfileCompleted: boolean;
}