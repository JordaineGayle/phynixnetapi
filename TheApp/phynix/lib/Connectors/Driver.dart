import 'dart:convert';
import 'dart:io';

import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class Driver {

  static String baseUrl = "https://phynixnetapi.azurewebsites.net";

  static broadcastLocation(Map<String, dynamic> map) async {
    SharedPreferences pref = await SharedPreferences.getInstance();
    String token = pref.getString("token");

    String url = "/api/CollectDriverInfo?code=xUn/Nntavk665LdT641AM2MRJUIlpZwwYI9Mbne8YSCVNWjEubxgYg==";

    http.Response response = await http.post(baseUrl + url, body: json.encode(map), headers: {"Content-Type": "application/json", HttpHeaders.authorizationHeader: "Bearer $token"});
    print(response.body);
    return response.body;
  }
  
  static acceptRider(Map<String, dynamic> map) async {
    SharedPreferences pref = await SharedPreferences.getInstance();
    String token = pref.getString("token");

    String url = "/api/AcceptRider?code=qdr/XZKiKvwdE6RdqM9LVvnNzpqr1z5XYOawZtbfGdZuRxJ/VJF1lA==";

    print(json.encode(map));
    
    http.Response response = await http.post(baseUrl + url, body: json.encode(map), headers: {"Content-Type": "application/json", HttpHeaders.authorizationHeader: "Bearer $token"});

    print(response.body);
    print(response.statusCode);
    print(map);
    return response.body;
  }
}