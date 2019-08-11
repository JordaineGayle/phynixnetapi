
import 'package:flutter/material.dart';
import 'package:phynix/Connectors/Auth.dart';
import 'package:phynix/Models/RegisterModel.dart';

class Register extends StatefulWidget {
  State<Register> createState() => RegisterState();
}

class RegisterState extends State<Register> {

  TextEditingController email = TextEditingController();
  TextEditingController fname = TextEditingController();
  TextEditingController lname = TextEditingController();
  TextEditingController phone = TextEditingController();
  TextEditingController password = TextEditingController();
  String arg;
  final _scaffoldKey = GlobalKey<ScaffoldState>();



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

    // TODO: implement build
    arg = ModalRoute.of(context).settings.arguments;
    print(arg);
    return Scaffold(
      key: _scaffoldKey,
      appBar: AppBar(),
      body: Container(
        margin: EdgeInsets.all(20),
        child: ListView(
          children: <Widget>[
            textField("First Name", fname, false),
            textField("Last Name", lname, false),
            textField("Phone", phone, false),
            textField("Email", email, false),
            textField("password", password, true),
            RaisedButton(
              onPressed: () async {
                RegisterModel reg = new RegisterModel();
                reg.FirstName = fname.text;
                reg.LastName = lname.text;
                reg.Email = email.text;
                reg.Password = password.text;
                reg.Type = arg;
                String res = await Auth.register(reg);
                final snackBar = SnackBar(content: Text(res),
                action: SnackBarAction(
                  label: 'Continue',
                  onPressed: (){
                    Navigator.pushNamedAndRemoveUntil(context, 'login', (Route<dynamic> route) => false);
                  },
                ),);
                _scaffoldKey.currentState.showSnackBar(snackBar);
              },
              child: Text("Register"),
            )
          ],
        ),
      ),
    );
  }

}