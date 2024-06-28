export interface LoggedInUser {
    userName: string;
    knownAs: string;
    token: string;
    gender: string
    profilePhotoUrl: string | undefined;
    roles: string[];
    isProfileCompleted: boolean;
    recaptchaToken: string;
}