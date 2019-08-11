
import 'package:flutter/material.dart';
import 'package:shared_preferences/shared_preferences.dart';

class Login extends StatefulWidget {
  State<Login> createState() => LoginState();
}

class LoginState extends State<Login> {

  LoginState(){
    checkUser();
  }

  checkUser() async {
    SharedPreferences pref = await SharedPreferences.getInstance();
    var token = pref.get('token');
    var type = pref.get('type');

    if(token != null) {

      if(type == "driver"){
        Navigator.pushNamedAndRemoveUntil(context, 'driverMap', (Route<dynamic> route) => false);
      }else if(type == "rider"){
        Navigator.pushNamedAndRemoveUntil(context, 'ryderMap', (Route<dynamic> route) => false);
      }


    }
  }

  @override
  Widget build(BuildContext context) {
    // TODO: implement build
    return Scaffold(
      appBar: AppBar(),
      body: Container(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceEvenly,
              children: <Widget>[
                SizedBox(
                  height: 60,
                  width: 140,
                  child: RaisedButton(
                    onPressed: (){
                      String type = "rider";

                      Navigator.pushNamed(context, 'loginForm', arguments: type);
                    },
                    child: Text("Rider"),
                  ),
                ),
                SizedBox(
                  height: 60,
                  width: 140,
                  child: RaisedButton(
                    onPressed: (){
                      String type = 'driver';
                      Navigator.pushNamed(context, 'loginForm', arguments: type);
                    },
                    child: Text("Driver"),
                  ),
                )
              ],
            )
          ],
        ),
      )
    );
  }

}