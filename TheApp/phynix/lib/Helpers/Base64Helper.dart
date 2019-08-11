
import 'dart:convert';

class Base64Helper {


  static encodeBase64(String plainString) {
    List encodedText = utf8.encode(plainString);
    String base64Str = base64.encode(encodedText);
    print(base64Str);
    return base64Str;
  }

  static decodeBase64(String base64String) {
    String decodedText = utf8.decode(base64.decode(base64String));
    return decodedText;
  }
}