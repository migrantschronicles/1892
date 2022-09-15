//
//  UnityPlugin-Bridging-Header.h
//  UnityIosPlugin
//
//  Created by macOS Developer 001 on 5/10/21.
//

#ifndef UnityPlugin_Bridging_Header_h
#define UnityPlugin_Bridging_Header_h

#include "UnityPlugin-Bridging-Header.h"

extern "C" {
    
#pragma mark - Functions
    
    // Helper method to create C string copy
    char* MakeStringCopy (const char* string) {
        if (string == NULL)
            return NULL;
       
        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);
        return res;
    }

    const void* MakeByteCopy(const void* bytes, int len) {
        if(bytes == NULL)
            return NULL;
        
        Byte* res = (Byte*)malloc(len);
        memcpy(res, bytes, len);
        return res;
    }

    // int* is an array, int** is pointer to array.
    void _createDocument(int pageWidth, int pageHeight) {
        [[UnityPlugin shared] CreateDocument: (CGFloat)pageWidth pageHeight: (CGFloat)pageHeight];
    }

    void _addPage() {
        [[UnityPlugin shared] AddPage];
    }

    void _drawText(const char* t, float x, float y, const char* settings) {
        [[UnityPlugin shared] DrawText:[NSString stringWithUTF8String:t] pos_x: x pos_y: y textSettings: [NSString stringWithUTF8String:settings]];
    }

    void _setTypeface(const char* name) {
        [[UnityPlugin shared] SetTypeface:[NSString stringWithUTF8String:name]];
    }

    void _drawImage(Byte* dataPtr, long dataSize, float x, float y, int width, int height) {
        NSData *imgData = [NSData dataWithBytes:dataPtr length:dataSize];
        [[UnityPlugin shared] DrawImage:imgData pos_x:x pos_y:y width:width height:height];
    }

    int _getDocumentData(int** dataPtr) {
        NSData* data = [[UnityPlugin shared] GetDocumentData];
        
        NSUInteger len = data.length;
        // Don't care about array type; cast to same pointer type.
        *dataPtr = (int*)MakeByteCopy(data.bytes, len);
        
        NSLog(@"Length: %li", len);
        return len;
    }
}

#endif /* UnityPlugin_Bridging_Header_h */
