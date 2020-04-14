Write-Host "Creating Certificates for Self-Signed Testing" 

Write-Host "Createing Root Certificate"
$cert = New-SelfSignedCertificate -Type Custom -KeySpec Signature `
-Subject "CN=localhost" `
-FriendlyName "gRPCDemoRootCert" `
-KeyExportPolicy Exportable `
-HashAlgorithm sha256 `
-KeyLength 4096 `
-CertStoreLocation "cert://currentuser/My" `
-KeyUsageProperty Sign `
-KeyUsage CertSign `
-NotAfter (Get-Date).AddYears(1)

# Client Auth
Write-Host "Createing Client Auth Certificate"
$clientCert = New-SelfSignedCertificate -Type Custom `
-KeySpec Signature `
-Subject "CN=localhost" `
-KeyExportPolicy Exportable `
-FriendlyName "gRPCDemoClientCert" `
-HashAlgorithm sha256 `
-KeyLength 2048 `
-NotAfter (Get-Date).AddMonths(12) `
-CertStoreLocation "cert://currentuser/My" `
-Signer $cert `
-TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.2")

# TLS Cert
Write-Host "Createing Web Server Certificate"
$webCert = New-SelfSignedCertificate -Type Custom `
-Subject "CN=localhost" `
-KeyExportPolicy Exportable `
-DnsName "localhost" `
-FriendlyName "gRPCDemoTlsCert" `
-HashAlgorithm sha256 `
-KeyLength 2048 `
-KeyUsage "KeyEncipherment", "DigitalSignature" `
-NotAfter (Get-Date).AddMonths(12) `
-CertStoreLocation "cert://currentuser/My" `
-Signer $cert

$PFXPass = ConvertTo-SecureString -String "P@ssw0rd!" -Force -AsPlainText

Write-Host "exporting Certificates to file"

Export-PfxCertificate -Cert $clientCert `
-Password $PFXPass `
-FilePath gRPCDemoSelfCert.pfx

Export-PfxCertificate -Cert $webCert `
-Password $PFXPass `
-FilePath gRPCDemoSslCert.pfx