<p align="center">
    <a href="http://cierge.biarity.me" target="_blank">
    <img alt="Cierge" title="Cierge" src="https://github.com/PwdLess/Cierge-Website/blob/master/Design/LogoBlack.png?raw=true" width="500">
    </a>
</p>
<p align="center">
Cierge is an OpenID Connect server that <b>handles user signup, login, profiles, management, and more.</b> Instead of storing passwords, Cirege uses <b>magic links/codes and external logins</b> to authenticate your users.
<br />
<a  href="http://cierge.biarity.me" target="_blank">🌐Visit our website🌐</a>
</p>


---

## Features

add features with screenshot for each in a table?

---

## Why Cierge

**😊No passwords to hash, no passwords to store.**

Even if your database is compromised, your users won't be.

**😊Users won't have to come up with their 278th password.**

Lack of complex password rules means convince for both you and your users. User won't have to come up with and remember yet another password, and you won't have to worry about password reuse.

**😊User management**

Users can edit their profiles and add or remove emails or external logins while an admin manages it all from the admin panel.

**😊External logins out of the box**

Instead of logging in using email, users can use social logins such as Google, Facebook, Github, etc. Users can also associate multiple logins for one account. External logins are very easy to setup, for example, to add Google logins simply paste your site key & secret into the configuration and Cierge does the rest!

**😌Brute forcing is no longer a problem.**

Cierge utilizes reCAPTCHA to ensure magic codes (which expire quickly) are not brute-forceable.

**😌No profile existence leakage. Actually, no leakage of any kind.**

With traditional password systems, a malicious user can try to register with an email to find out if it exists. With Cierge, 0 data is leaked about users or if they exist - until authenticated. This comes naturally since Cierge makes little distinction between registration and login.

---

## FAQ

**😟What if a user's email is compromised?**

That's also a problem with traditional password logins. An attacker can click "forgot password", enter an email, and simply bypass the password altogether. As a matter of fact, Cierge removes a point of failure by making passwordless the only login method.

**😲What if my email is only accessible on another device?**

Cierge sends a magic link as well as a magic code that a user can manually enter into the login screen to continue as an alternative to clicking the link. Magic codes are short, volatile, & memorable (eg. 443 863). For example, you can look up the code on your phone then enter it into your browser on desktop. Note that Cierge also allows external social logins so users can skip emails altogether.

**😫I don't find this convenient enough! And what about grey listing!**

Cierge supports external social logins (eg. Google, Facebook, Twitter, Github, etc.) in addition to email login. Users can use any number of login methods at the same time. Also remember that Cierge is, if anything, more convenient than the now-popular 2FA.

**🤔How does Cierge handle changing emails?**

Cierge does not have a "change email" feature. Instead, users can "add" or "remove" logins (logins can be emails or external logins) - so changing an email is equivalent to adding a new email (which involves verifying it) then optionally removing the old one. This ensures users can't use unverified emails, and makes it hard for an intruder to completely take ownership of an account. Removing your last login is equivalent to deleting your account.

**🤔What about breach detection?**

With traditional password logins, a user would notice if their password has been changed. With Cierge, a user would notice if an attacker removed their email from thier logins. In addition, Cierge exposes an easily-accessible read-only event log of everything that has happened to an account (with associated IP addresses & user agents) to aid in breach detection, accessible to account owners and admins.