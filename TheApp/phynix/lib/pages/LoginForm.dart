
import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:phynix/Connectors/Auth.dart';

class LoginForm extends StatefulWidget {
  State<LoginForm> createState() => LoginFormState();
}

class LoginFormState extends State<LoginForm> {

  String arg;
  TextEditingController email = TextEditingController();
  TextEditingController password = TextEditingController();


  textField(String label, TextEditingController control, bool secure) {
    return Container(
      margin: EdgeInsets.symmetric(vertical: 18),
      child: TextField(
        obscureText: secure,
        controller: control,
        decoration: InputDecoration(
            hintText: label,
            enabledBorder: OutlineInputBorder(
                borderSide: BorderSide(color: Colors.black, width: .5),
                borderRadius: BorderRadius.all(Radius.circular(3))
            )
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {

    arg = ModalRoute.of(context).settings.arguments;
    print(arg);

    // TODO: implement build
    return Scaffold(
      appBar: AppBar(),
      body: Container(
        margin: EdgeInsets.all(15),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: <Widget>[
            textField("Email", email, false),
            textField("Password", password, true),
            RaisedButton(
              onPressed: () async {
                String tokenString = await Auth.login(email.text, password.text, arg);
                
                Map<String, dynamic> map = json.decode(tokenString);
                
                if(map["result"]["token"] != null){

                  if(arg == 'driver'){
                    Navigator.pushNamedAndRemoveUntil(context, 'driverMap', (Route<dynamic> route) => false);
                  }else{
                    Navigator.pushNamedAndRemoveUntil(context, 'ryderMap', (Route<dynamic> route) => false);
                  }

                }
              },
              child: Text("Login"),
            ),
            GestureDetector(
              onTap: (){
                Navigator.pushNamed(context, 'registerForm', arguments: arg);
              },
              child: Padding(
                padding: EdgeInsets.all(18),
                child: Text("Register"),
              ),
            )
          ],
        ),
      )
    );
  }

}
