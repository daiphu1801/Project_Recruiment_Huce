# Hướng dẫn chia sẻ Localhost với Ngrok
ngrok http 44300 --host-header=rewrite

# Với cổng https
ngrok http https://localhost:44335 --host-header=localhost
