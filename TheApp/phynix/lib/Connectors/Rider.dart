import 'dart:convert';
import 'dart:io';

import 'package:google_maps_flutter/google_maps_flutter.dart';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';

class Ryder {
  static String baseUrl = "https://phynixnetapi.azurewebsites.net";


  static Future<String> submitLocation(Map<String, dynamic> map) async {

    SharedPreferences pref = await SharedPreferences.getInstance();
    String token = pref.getString("token");

    http.Response response = await http.post(baseUrl + "/api/BroadcastMyAddress?code=vdmS0lPcGJo/0iwNKT/E0MWbaicY2Dxrf9ApFv/XyzbDQ2jJ/gUtbg==", body: json.encode(map), headers: {"Content-Type": "application/json", HttpHeaders.authorizationHeader: "Bearer $token"});

    print("broadcast ${response.body}");
    print(response.statusCode);
    print(map);

    return response.body;
  }

}