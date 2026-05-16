$response = Invoke-WebRequest -Uri "https://source.unsplash.com/800x600/?theater" -MaximumRedirection 0 -ErrorAction Ignore
if ($response.StatusCode -eq 302) {
    Write-Output $response.Headers.Location
} else {
    Write-Output "Failed"
}
