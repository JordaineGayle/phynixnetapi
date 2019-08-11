import 'package:flutter/material.dart';
import 'package:phynix/pages/Driver/DriverMap.dart';
import 'package:phynix/pages/Login.dart';
import 'package:phynix/pages/LoginForm.dart';
import 'package:phynix/pages/Register.dart';
import 'package:phynix/pages/Ryder/Map.dart';

void main() => runApp(MaterialApp(
  home: Login(),
  routes: <String, WidgetBuilder>{
    'login':(BuildContext context) => Login(),
    'loginForm': (BuildContext context) => LoginForm(),
    'registerForm': (BuildContext context) => Register(),
    'ryderMap': (BuildContext context) => RMap(),
    'driverMap': (BuildContext context) => DriverMap()
  },
));

