// The Cloud Functions for Firebase SDK to create Cloud Functions and setup triggers.
const functions = require('firebase-functions');

// // Create and Deploy Your First Cloud Functions
// // https://firebase.google.com/docs/functions/write-firebase-functions
//
// exports.helloWorld = functions.https.onRequest((request, response) => {
//  response.send("Hello from Firebase!");
// });

// Initialize Admin
const admin = require('firebase-admin');
const adminAccountJson = require('./【Firebase アプリ名】-firebase-adminsdk.json');
admin.initializeApp({
  credential: admin.credential.cert(adminAccountJson),
  databaseURL: "https://【Firebase アプリ名】.firebaseio.com",
});
// Initialize Firebase
const firebaseConfig = {
  // TODO: ウェブアプリに Firebase を追加
}
const firebase = require("firebase");
firebase.initializeApp(firebaseConfig);

// 
// --user
// 

// ユーザー作成して自動でログイン処理
exports.createAccount = functions.https.onRequest((req, res) => {
  if (typeof req.query.email === 'undefined' || typeof req.query.password === 'undefined') {
    return res.send('{"result":false, "error":"Not found email or password"}');
  }

  var account = {
    email: req.query.email, 
    password: req.query.password
  }
  // create
  firebase.auth().createUserWithEmailAndPassword(account.email, account.password).catch(function(error) {
    if (typeof error === 'undefined') {
      return res.send('{"result":true}');
    }
    else {
      return res.send('{"result":false, "error":' + JSON.stringify(error) + '}');
      }
  });
});

// ログイン処理
exports.loginUser = functions.https.onRequest((req, res) => {
  if (typeof req.query.email === 'undefined' || typeof req.query.password === 'undefined') {
    return res.send('{"result":false, "error":"Not found email or password"}');
  }

  var account = {
    email: req.query.email, 
    password: req.query.password
  }
  // login
  firebase.auth().signInWithEmailAndPassword(account.email, account.password).catch(function(error) {
    if (typeof error === 'undefined') {
      return res.send('{"result":true}');
    }
    else {
      return res.send('{"result":false, "error":' + JSON.stringify(error) + '}');
      }
  });
});

// ユーザー情報取得
exports.getUserState = functions.https.onRequest((req, res) => {
  if (typeof req.query.email === 'undefined' || typeof req.query.password === 'undefined') {
    return res.send('{"result":false, "error":"Not found email or password"}');
  }

  var account = {
    email: req.query.email, 
    password: req.query.password
  }

  // login
  firebase.auth().signInWithEmailAndPassword(account.email, account.password).catch(function(error) {
    if (typeof error !== 'undefined') {
      return res.send('{"result":false, "error":' + JSON.stringify(error) + ']"}');
      }
    else
    {
      // // get status
      // firebase.auth().onAuthStateChanged(function(user) {
      //   if (user) {
      //     var user_data = {
      //       displayName: user.displayName,
      //       email: user.email,
      //       emailVerified: user.emailVerified,
      //       photoURL: user.photoURL,
      //       isAnonymous: user.isAnonymous,
      //       uid: user.uid,
      //       // providerData: user.providerData,
      //     }
      //     return res.send('{"result":true, "user":' + JSON.stringify(user_data) + '}');
      //   } else {
      //     return res.send('{"result":false, "error":"Not found user data."}');
      //   }
      // });
      
      var user_data = getUserData();
      return res.send('{"result":true, "user":' + JSON.stringify(user_data) + '}')
    }
  });
});

// ユーザーの詳細を取得
function getUserData() {
  // get status
  var user = firebase.auth().currentUser;
  var result = {};
  if (user) {
    result = {
      displayName: user.displayName,
      email: user.email,
      emailVerified: user.emailVerified,
      photoURL: user.photoURL,
      isAnonymous: user.isAnonymous,
      uid: user.uid,
      // providerData: user.providerData,
    };
  }

  return result;
}

// カスタムトークン発行
exports.createCustomToken = functions.https.onRequest((req, res) => {
  if (typeof req.query.uid === 'undefined') {
    return res.send('{"result":false, "error":"Not found uid"}');
  }

  var uid = req.query.uid;
  var additionalClaims = {
    premiumAccount: true
  };
  admin.auth().createCustomToken(uid, additionalClaims)
    .then(function(customToken) {
      // return res.redirect(303, customToken);
      // return res.send(customToken);
      // getUserData((user_data) => {
      //   user_data.customToken = customToken;
      //   return res.send('{"result":true, "user":"' + JSON.stringify(user_data) + '"}');
      // });
      return res.send('{"result":true, "token":"' + customToken + '"}');
    })
    .catch(function(error) {
      // console.log("Error creating custom token:", error);
      // return res.redirect(500, "Error creating custom token: " + error);
      return res.send('{"result":false, "error":' + JSON.stringify(error) + ']"}');
    });

});

// 
// --ranking
// 

// ユーザーの獲得ポイント保存
exports.addUserPoint = functions.https.onRequest((req, res) => {
  if (typeof req.query.uid === 'undefined' || typeof req.query.point === 'undefined') {
      return res.send('{"result":false, "error":"Not found uid or point"}');
  }
  // var user = firebase.auth().currentUser;
  // if (typeof req.query.uid !== user.uid) {
  //   return res.send('{"result":false, "error":"Error uid."}');
  // }

  var user_name = 'no_name';
  if (typeof req.query.name !== 'undefined'){
    user_name = req.query.name;
  }
  var new_data = {
    name: user_name,
    point: Number(req.query.point)
  };
  admin.firestore().doc('rankings/'+req.query.uid).set(new_data)
  .then(function() {
    addRanking();
    return res.send('{"result":true}');
  })
  .catch(function(error) {
    return res.send('{"result":false, "error":' + JSON.stringify(error) + ']"}');
  });
});

// ランキングデータを更新
exports.getUserPoint = functions.https.onRequest((req, res) => {
  // 全ユーザーの最新情報を取得
  admin.firestore().collection('rankings').get()
  .then((snapshot) => {
    // var rankingList = {}
    // snapshot.forEach((doc) => {
    //   // console.log(doc.id, '=>', doc.data());
    //   rankingList[''+doc.id] = doc.data();
    // });
    // return res.send('{"result":true}');
    return res.send('{"result":true, "ranking":' + JSON.stringify(snapshot) + '}');
  })
  .catch((error) => {
    throw res.send('{"result":false, "error":' + JSON.stringify(error) + '}');
  });
});

// ランキングデータに値を新たに保存
exports.addRanking = functions.https.onRequest((req, res) => {
  if (typeof req.query.uid === 'undefined' || typeof req.query.point === 'undefined') {
    return res.send('{"result":false, "error":"Not found uid or point"}');
  }
  // var user = firebase.auth().currentUser;
  // if (typeof req.query.uid !== user.uid) {
  //   return res.send('{"result":false, "error":"Error uid."}');
  // }

  var user_name = 'no_name';
  if (typeof req.query.name !== 'undefined'){
    user_name = req.query.name;
  }
  var new_data = {
    name: user_name,
    point: Number(req.query.point)
  };
  admin.database().ref('rankings/'+req.query.uid).set(
    new_data
    ,function(error) {
      if (error) {
        return res.send('{"result":false, "error":' + JSON.stringify(error) + '}');
      }
      else {
        return res.send('{"result":true}');
      }
    }
  );
});

// ユーザーの獲得ポイント保存
exports.updateRanking = functions.https.onRequest((req, res) => {
  // if (typeof req.query.uid === 'undefined' || typeof req.query.point === 'undefined') {
  //     return res.send('{"result":false, "error":"Not found uid or point"}');
  // }
  // var user = firebase.auth().currentUser;
  // if (typeof req.query.uid !== user.uid) {
  //   return res.send('{"result":false, "error":"Error uid."}');
  // }
  admin.database().ref('rankings').on('value', function(snapshot) {
    // console.info("snapshot: " + JSON.stringify(snapshot))
    // 最新のランキングデータを取得
    var updateRankingList = [];
    snapshot.forEach((ranking_data) => {
      updateRankingList.push(ranking_data);
    });
    var updateRankings = orderByRanking(updateRankingList);

    // update
    var error = false;
    for(var key in updateRankings) {
      var next_ranking = {};
      console.info("updateRankings["+key+"]: " + JSON.stringify(updateRankings[key]));
      next_ranking['/rankings/'+key] = updateRankings[key];
      admin.database().ref().update(
        next_ranking,
        function(err) {
          if(err) error = true;
        }
      );
    }
    if (error) {
      return res.send('{"result":false, "error":' + JSON.stringify(error) + '}');
    }
    else {
      return res.send('{"result":true}');
    }    
  });
});

// ランキング10を決める
const rankingMax = 10;
function orderByRanking(updateRankingList) {
  console.info("orderByRanking_Before: " + JSON.stringify(updateRankingList));
  // updateRankingList.sort(function(a,b){
  //   if(Number(a.point) > Number(b.point)) return -1;
  //   if(Number(a.point) < Number(b.point)) return 1;
  //   return 0;
  // });
  updateRankingList.sort(
    function(a,b){
      return (Number(a.point) < Number(b.point) ? 1 : -1);
    }
  );
  console.info("orderByRanking_After: " + JSON.stringify(updateRankingList));

  var rankingSize = updateRankingList.length;
  if(rankingSize > rankingMax) {
    rankingSize = rankingMax;
  }
  var nextRankings = {};
  for(var i = 0; i<rankingSize; i++) {
    nextRankings[""+(i+1)] = updateRankingList[i];
  }

  return nextRankings;
}

// 全ランキングデータ取得
exports.getRanking = functions.https.onRequest((req, res) => {
  // ランキングデータを取得
  admin.database().ref('rankings').on('value', function(snapshot) {
    console.log(JSON.stringify(snapshot));
    var updateRankingList = [];
    snapshot.forEach((ranking_data) => {
      updateRankingList.push(ranking_data);
    });
    // var updateRankings = orderByRanking(updateRankingList);
    // console.info(JSON.stringify(updateRankings));

    // snapshot.forEach((doc) => {
    //   console.info(JSON.stringify(doc));
    // });
    return res.send('{"result":true, "ranking":' + JSON.stringify(snapshot) + '}');
  });
});




