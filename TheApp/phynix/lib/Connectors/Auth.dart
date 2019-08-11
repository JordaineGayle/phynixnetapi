import 'dart:convert';
import 'dart:io';

import 'package:http/http.dart' as http;
import 'package:phynix/Helpers/Base64Helper.dart';
import 'package:phynix/Models/RegisterModel.dart';
import 'package:shared_preferences/shared_preferences.dart';

class Auth {
  static String baseUrl = "https://phynixnetapi.azurewebsites.net";

  static Future<String> register(RegisterModel reg) async {

    Map<String, dynamic> map = new Map<String, dynamic>();

    map['FirstName'] = reg.FirstName;
    map['LastName'] = reg.LastName;
    map['Email'] = reg.Email;
    map['Phone'] = reg.Phone;
    map['Password'] = reg.Password;
    map['Type'] = reg.Type;
    print(map);

    String endpoints;
    if(reg.Type == "driver"){
      endpoints = "/api/CreateDriver?code=1WrwVsuHSJxTW5zamazbR7GLestPqysfrxQUt3anCfXq9A2vyy95MQ==";
    }else{
      endpoints = "/api/CreateRider?code=hfZSyH2h0aHiLT2fEkQP5DwuAhrrBAtzmPkZgKnaQq7XyEYsMH5NDA==";
    }

    http.Response response = await http.post(baseUrl + endpoints, body: json.encode(map), headers:  {"Content-Type": "application/json"});

    print(response.body);
    print(response.statusCode);

    return response.body;
  }

  static Future<String> login(String email, String password, String type) async {


    String encoded = "$email:$password:$type";
    String encodedString = Base64Helper.encodeBase64(encoded);

    print(type);

    http.Response response  = await http.get(baseUrl + '/api/Login?token=$encodedString');

    print(response.body);

    Map<String, dynamic> token = json.decode(response.body);
    SharedPreferences pref = await SharedPreferences.getInstance();

    pref.setString('token', token['result']['token']);
    pref.setString('user', json.encode(token['result']['data']));
    pref.setString('type', type);
    return response.body;
  }
  
  static Future<dynamic> signalRAuth () async {

    SharedPreferences pref = await SharedPreferences.getInstance();
    String token = pref.getString("token");
    
    http.Response response  = await http.post(baseUrl + "/api/DriverAuth?code=WqrlABiFrP2kiD8UIDybfUvya3mHRGN2mW7q7EfA3mkt1by5Hc7HwQ==", headers: {HttpHeaders.authorizationHeader: "Bearer $token"});

    print('hello' + response.body);
    print(response.statusCode);
    return json.decode(response.body);
  }
}