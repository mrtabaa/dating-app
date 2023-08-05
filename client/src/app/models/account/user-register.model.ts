export interface UserRegister{
    email: string | null;
    password: string | null;
    confirmPassword: string | null;
    dateOfBirth: string | undefined,
    knownAs: string,
    gender: string,
    city: string,
    country: string
}