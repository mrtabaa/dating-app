export interface LoggedInUser {
  email: string; // Used only to verify account. Will always be null if account is verified.
  userName: string;
  knownAs: string;
  token: string; // Replace with ROLES
  gender: string
  profilePhotoUrl: string | undefined;
  roles: string[];
  isProfileCompleted: boolean;
  recaptchaToken: string;
  isEmailNotConfirmed: boolean;
}
