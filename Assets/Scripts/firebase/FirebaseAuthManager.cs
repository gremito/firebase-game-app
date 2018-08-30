using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using Firebase;
using Firebase.Auth;
using Firebase.Functions;

[Serializable]
public class CustomToken {
    public bool result;
    public string token;
}

public class FirebaseAuthManager
{

    private FirebaseAuth auth = null;
    private FirebaseUser user = null;
	private FirebaseFunctions firebaseFunctions = null;

    public FirebaseApp FirebaseApp {
        get { 
           if(auth != null) {
               return auth.App;
           }
           else {
               return null;
           }
        }
    }

    public FirebaseFunctions Function {
        get { 
           if(firebaseFunctions != null) {
               return firebaseFunctions;
           }
           else {
               return null;
           }
        }
    }

    private string userToken = String.Empty;

    public FirebaseAuthManager()
    {
    }

    /// <summary>
    /// FirebaseAuthの初期化
    /// </summary>
    public void InitializeFirebase()
    {
        if(auth != null)  return;
        Debug.Log("Setting up Firebase Auth & Functions");

        auth              = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

        if(firebaseFunctions == null) {
            firebaseFunctions = FirebaseFunctions.GetInstance(auth.App);
        }        
    }

    /// <summary>
    /// ユーザーデータを取得する
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    public void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if(auth == null)  return;

        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                // get token
                createCustomToken(user.UserId);
            }
        }
    }

    /// <summary>
    /// カスタムトークン（認証トークン）を取得
    /// </summary>
    /// <memo>
    /// http://andrew.korobov.com/andrew/surf.aspx?dec=1&url=uh4QwhlWmOZCurjBsCfPt5VDvSZDvgoKsSZJmS4LsTlLtDpKsT4FvSVPmS07vgN7sCNB2S7I2q4BB6X!
    /// </memo>
    private Task<string> createCustomToken(string userId) {
        // Create the arguments to the callable function.
        var data = new Dictionary<string, object>();
        data["uid"] = userId;

        // Call the function and extract the operation from the result.
        var function = firebaseFunctions.GetHttpsCallable("createCustomToken");
        
        return function.CallAsync(data).ContinueWith((task) => {
            string result = (string) task.Result.Data;
            Debug.Log("createCustomToken result: " + result);
            CustomToken customTokentoken = JsonUtility.FromJson<CustomToken>(result);
            if(customTokentoken.token != null) {
                userToken = customTokentoken.token;
            }
            return (string) task.Result.Data;
        });
    }

    public void OnDestroy()
    {
        if(auth == null)  return;
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    /// <summary>
    /// 新しいユーザーを登録する
    /// </summary>
    public void CreateUserWithEmailAndPassword(string email, string password)
    {
        if(auth == null)  return;
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if(task == null) return;
            if(task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if(task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            user = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", user.DisplayName, user.UserId);

            // TODO: userIdをキャッシュまたは保存

        });
    }

    /// <summary>
    /// ユーザーをログインさせる
    /// </summary>
    public void SignInWithEmailAndPassword(string email, string password)
    {
        if(auth == null)  return;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {

            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            user = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",user.DisplayName, user.UserId);

        });		
    }

    /// <summary>
    /// 再ログイン
    /// </summary>
    public void Reauthenticate(string email, string password)
    {
        if(auth == null)  return;

        Firebase.Auth.Credential credential = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);

          if (user != null)
          {
            user.ReauthenticateAsync(credential).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("ReauthenticateAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("ReauthenticateAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User reauthenticated successfully.");
            });
            }
    }

    /// <summary>
    /// カスタムトークンでサインイン
    /// </summary>
    public void SignInWithCustomTokenAsync(string custom_token)
    {
        if(auth == null && custom_token == null)  return;

        auth.SignInWithCustomTokenAsync(custom_token).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithCustomTokenAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithCustomTokenAsync encountered an error: " + task.Exception);
                return;
            }

            user = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",user.DisplayName, user.UserId);

        });
    }

    /// <summary>
    /// ユーザーIDを取得
    /// </summary>
    public void GetUserId()
    {
        if(auth == null)  return;
        user = auth.CurrentUser;
        if (user != null) {
            string name           = user.DisplayName;
            string email          = user.Email;
            System.Uri photo_url  = user.PhotoUrl;
            string uid            = user.UserId;
        }    
    }

}
