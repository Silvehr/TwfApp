dotnet build -c Release -f net10.0-android -p:AndroidKeyStore=false

if(-not $?)
{
    Write-Error "Build failed. Aborting..."
    return;
}

if(-not (Test-Path "./android-publish")){
    New-Item -Type Directory "./android-publish"
}

$dir = New-Item -Type Directory "./android-publish/build_$(([System.DateTime]::Now).ToString('yyyy-MM-dd_hh-mm-ss'))"

& "/Program Files (x86)/Android/android-sdk/build-tools/36.0.0/zipalign.exe" -p -v 4 "./TwfApp/bin/Release/net10.0-android/org.twf.kierownikapp.apk" $(Join-Path $dir "org.twf.kierownikapp-aligned.apk")
& "/Program Files (x86)/Android/android-sdk/build-tools/36.0.0/apksigner.bat" sign --ks "/Users/maksy/AndroidKeys/kierownikapp.keystore" --ks-key-alias kierownikapp --out $(Join-Path $dir "org.twf.kierownikapp-signed.apk") $(Join-Path $dir "org.twf.kierownikapp-aligned.apk")