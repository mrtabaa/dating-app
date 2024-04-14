export interface UserRegister{
    email: string;
    username: string;
    password: string;
    confirmPassword: string;
    dateOfBirth: string | undefined;
    knownAs: string;
    gender: string;
    city: string;
    country: string;
}