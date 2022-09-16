//
//  UnityPlugin.swift
//  UnityIosPlugin
//
//  Created by macOS Developer 001 on 5/10/21.
//

import Foundation
import PDFKit

@objc public class UnityPlugin : NSObject {
    
    public var pdfRenderer = UIGraphicsPDFRenderer()
    public var pdfData = NSMutableData()
    @objc public static let shared = UnityPlugin()
    var currentFontName = UIFont.systemFont(ofSize: 12).fontName
    
    @objc public func CreateDocument(_ pageWidth: CGFloat, pageHeight: CGFloat)
    {
        let renderer = UIPrintPageRenderer()
        let pageRect = CGRect(x: 0, y: 0, width: pageWidth, height: pageHeight)
        
        pdfData = NSMutableData()
        UIGraphicsBeginPDFContextToData(pdfData, pageRect, nil)
        
        if(UIGraphicsGetCurrentContext() == nil)
        {
            NSLog("Current context is NIL")
        }
		
        renderer.prepare(forDrawingPages: NSMakeRange(0, renderer.numberOfPages))
    }
    
    @objc public func AddPage()
    {
        UIGraphicsBeginPDFPage()
    }
    
    @objc public func DrawText(_ text: String, pos_x:CGFloat, pos_y:CGFloat, textSettings:String)
    {
        let parseSettings = textSettings.replacingOccurrences(of: "(", with: "").replacingOccurrences(of: ")", with: "")
        let settingsParams = parseSettings.components(separatedBy: ",")

        guard let textSize = NumberFormatter().number(from: settingsParams[3]) else { return }
        guard let colorR = NumberFormatter().number(from: settingsParams[0]) else { return }
        guard let colorG = NumberFormatter().number(from: settingsParams[1]) else { return }
        guard let colorB = NumberFormatter().number(from: settingsParams[2]) else { return }
        let underline = (settingsParams[4] as NSString).boolValue;
        
        UIGraphicsGetCurrentContext()
            let attributes = [
                NSAttributedString.Key.font : UIFont(name: currentFontName, size: CGFloat(truncating: textSize)),
                NSAttributedString.Key.foregroundColor : UIColor.init(red: CGFloat(colorR) / 255, green: CGFloat(colorG) / 255, blue: CGFloat(colorB) / 255, alpha: 1),
                NSAttributedString.Key.underlineStyle : underline ? 1 : 0
            ] as [NSAttributedString.Key : Any]
            
        text.draw(at: CGPoint(x: pos_x, y: pos_y), withAttributes: attributes)
    }
    
    @objc public func SetTypeface(_ name:String)
    {
        
        for fontFamily in UIFont.familyNames {
            NSLog(fontFamily)
            for fontName in UIFont.fontNames(forFamilyName: fontFamily){
                if(name == fontName){
                    NSLog("FONT: " + fontName + ", FAMILY: " + fontFamily)
                    currentFontName = fontFamily
                }
            }
        }
        
        NSLog("Font name: " + currentFontName)
    }
    
    @objc public func GetAvailableFontFamilies() -> [String]
    {
        return UIFont.familyNames
    }
    
    @objc public func DrawImage(_ imgData: NSData, pos_x:CGFloat, pos_y:CGFloat, width:Int, height:Int)
    {

        let image = UIImage(data: Data(referencing: imgData))
        if(image == nil)
        {
            NSLog("IMAGE IS NIL")
        }
        image?.draw(in: CGRect(x: pos_x, y: pos_y, width: CGFloat(width), height: CGFloat(height)))
    }
    
    @objc public func GetDocumentData() -> NSData {
        
        UIGraphicsEndPDFContext()        
        return pdfData
    }
}
