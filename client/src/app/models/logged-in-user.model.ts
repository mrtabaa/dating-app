export interface LoggedInUser {
  email: string; // Used only to verify account. Will always be null if account is verified.
  rolesStr: string[];
  userName: string;
  knownAs: string;
  gender: string; // TODO: Replace with enum
  profilePhotoUrl: string | undefined;
  isProfileCompleted: boolean;
  recaptchaToken: string;
  isEmailNotConfirmed: boolean;
}
