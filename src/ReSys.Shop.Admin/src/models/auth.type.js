// Login:
/**
 * Immutable parameter holder (similar to a sealed C# record).
 */
class LoginParam {
    /**
     * @param {string} credential - The credential value.
     * @param {string} password - The password value.
     * @param {boolean} [rememberMe=false] - Flag indicating whether to remember the user.
     */
    constructor(credential, password, rememberMe = false) {
        this.credential = credential;
        this.password = password;
        this.rememberMe = rememberMe;

        // Freeze the instance to make it immutable, emulating a sealed record.
        Object.freeze(this);
    }
}

// User Registration:
/**
 * Parameter holder for user registration (similar to a C# class).
 */
class RegisterParam {
    /**
     * @param {Object} [options] - Optional initialization values.
     * @param {string|null} [options.userName] - User name (nullable).
     * @param {string} [options.email] - Email address (defaults to empty string).
     * @param {string|null} [options.firstName] - First name (nullable, defaults to empty string).
     * @param {string|null} [options.lastName] - Last name (nullable, defaults to empty string).
     * @param {string|null} [options.phoneNumber] - Phone number (nullable).
     * @param {string|null} [options.confirmPassword] - Confirmation password (non‑nullable in C#, initialized with null!).
     * @param {string|null} [options.password] - Password (non‑nullable in C#, initialized with null!).
     * @param {Date|null} [options.dateOfBirth] - Date of birth (nullable).
     */
    constructor({
        userName = null,
        email = '',
        firstName = '',
        lastName = '',
        phoneNumber = null,
        confirmPassword = null,
        password = null,
        dateOfBirth = null,
    } = {}) {
        this.userName = userName;                 // string|null
        this.email = email;                       // string
        this.firstName = firstName;               // string|null
        this.lastName = lastName;                 // string|null
        this.phoneNumber = phoneNumber;           // string|null
        this.confirmPassword = confirmPassword;   // string|null (C# non‑nullable, set later)
        this.password = password;                 // string|null (C# non‑nullable, set later)
        this.dateOfBirth = dateOfBirth;           // Date|null
    }
}

// Sesssion:
class SessionResult {
    /**
     * @param {Object} [options] - Optional initial values.
     * @param {string} [options.userId=''] - The unique identifier of the user.
     * @param {string} [options.userName=''] - The display name of the user.
     * @param {string} [options.email=''] - The email address of the user.
     * @param {string|null} [options.phoneNumber=null] - The phone number of the user (nullable).
     * @param {boolean} [options.isEmailConfirmed=false] - Flag indicating if the email is confirmed.
     * @param {boolean} [options.isPhoneNumberConfirmed=false] - Flag indicating if the phone number is confirmed.
     * @param {Array<string>} [options.roles=[]] - List of role names assigned to the user.
     * @param {Array<string>} [options.permissions=[]] - List of permission strings granted to the user.
     */
    constructor(options = {}) {
        this.userId = options.userId ?? '';
        this.userName = options.userName ?? '';
        this.email = options.email ?? '';
        this.phoneNumber = options.phoneNumber ?? null; // nullable
        this.isEmailConfirmed = options.isEmailConfirmed ?? false;
        this.isPhoneNumberConfirmed = options.isPhoneNumberConfirmed ?? false;
        this.roles = Array.isArray(options.roles) ? options.roles : [];
        this.permissions = Array.isArray(options.permissions) ? options.permissions : [];
    }
}

class RefreshTokenParam {
  /**
   * Creates a new RefreshTokenParam instance.
   * @param {string} refreshToken - The refresh token value.
   * @param {boolean} [rememberMe=false] - Optional flag indicating whether to remember the user.
   */
  constructor(refreshToken, rememberMe = false) {
    this._refreshToken = refreshToken;
    this._rememberMe = rememberMe;

    // Make the instance immutable, similar to a C# record
    Object.freeze(this);
  }

  /** Gets the refresh token. */
  get refreshToken() {
    return this._refreshToken;
  }

  /** Gets the remember‑me flag. */
  get rememberMe() {
    return this._rememberMe;
  }
}

export { 
  LoginParam, 
  RegisterParam, 
  SessionResult, 
  RefreshTokenParam }