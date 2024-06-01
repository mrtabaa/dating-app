export interface UserRegister {
    email: string;
    username: string;
    password: string;
    confirmPassword: string;
    dateOfBirth: string | undefined;
    gender: string;
    turnsTileToken: string;
}