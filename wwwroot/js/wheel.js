document.addEventListener('DOMContentLoaded', function () {
    const userIdentity = document.body.dataset.user;
    if (!userIdentity) return; // Kullanıcı giriş yapmamışsa çık.

    const wheelStorageKey = 'Etkincity_WheelSpun_' + userIdentity;
    
    // Eğer daha önce çarkı gördüyse (kapattıysa veya çevirdiyse) gösterme.
    if (localStorage.getItem(wheelStorageKey)) {
        return;
    }

    // Modal'ı göster
    const wheelModalElement = document.getElementById('wheelModal');
    if (!wheelModalElement) return;
    
    const wheelModal = new bootstrap.Modal(wheelModalElement, {
        backdrop: 'static',
        keyboard: false
    });
    
    wheelModal.show();

    // Kapatma butonuna basıldığında bir daha gösterme
    document.getElementById('closeWheelBtn').addEventListener('click', function() {
        localStorage.setItem(wheelStorageKey, 'closed');
        wheelModal.hide();
    });

    // Çark Ayarları
    const canvas = document.getElementById('wheelCanvas');
    const ctx = canvas.getContext('2d');
    const spinBtn = document.getElementById('spinBtn');
    
    const sectors = [
        { color: '#FF5252', label: '%10 İndirim', code: 'ETK-10' },
        { color: '#4CAF50', label: '%20 İndirim', code: 'ETK-20' },
        { color: '#2196F3', label: 'Pas', code: '' },
        { color: '#FFEB3B', label: '1 Bedava Bilet', code: 'ETK-FREE' },
        { color: '#9C27B0', label: 'VIP Yükseltme', code: 'ETK-VIP' },
        { color: '#FF9800', label: '%50 İndirim', code: 'ETK-50' },
    ];

    const numSectors = sectors.length;
    const arc = Math.PI * 2 / numSectors;
    const canvasSize = canvas.width;
    const radius = canvasSize / 2;

    // Çarkı Çiz
    function drawWheel() {
        ctx.clearRect(0, 0, canvasSize, canvasSize);
        
        for (let i = 0; i < numSectors; i++) {
            const angle = i * arc;
            ctx.beginPath();
            ctx.fillStyle = sectors[i].color;
            ctx.moveTo(radius, radius);
            ctx.arc(radius, radius, radius, angle, angle + arc);
            ctx.lineTo(radius, radius);
            ctx.fill();
            
            // Metni Ekle
            ctx.save();
            ctx.translate(radius, radius);
            ctx.rotate(angle + arc / 2);
            ctx.textAlign = 'right';
            ctx.fillStyle = '#fff';
            ctx.font = 'bold 16px sans-serif';
            ctx.shadowColor = 'rgba(0,0,0,0.5)';
            ctx.shadowBlur = 4;
            ctx.fillText(sectors[i].label, radius - 20, 5);
            ctx.restore();
        }
    }

    drawWheel();

    // Çevirme İşlemi
    let currentRotation = 0;
    
    spinBtn.addEventListener('click', function() {
        spinBtn.disabled = true;
        
        // Rastgele dönüş sayısı ve ekstra açı
        const spinRounds = Math.floor(Math.random() * 5) + 5; // 5 ile 10 tur arası
        const extraDegrees = Math.floor(Math.random() * 360);
        const totalDegrees = (spinRounds * 360) + extraDegrees;
        
        currentRotation += totalDegrees;
        
        // CSS animasyonu ile döndür
        canvas.style.transform = `rotate(${currentRotation}deg)`;
        
        // Animasyon bitiminde sonucu hesapla (CSS transition 4s)
        setTimeout(() => {
            // Son açı (0-360)
            const finalAngle = currentRotation % 360;
            // İşaretçi üstte (270 derece konumunda) olduğu için hesaplama:
            // Çark saat yönünde dönüyor. İşaretçi 270 derecede sabit.
            // Bu nedenle kazanılan dilimi bulmak için 360 - (finalAngle + 90) % 360
            let normalizedAngle = (360 - (finalAngle + 90) % 360) % 360;
            if (normalizedAngle < 0) normalizedAngle += 360;
            
            const winningIndex = Math.floor(normalizedAngle / (360 / numSectors));
            const winner = sectors[winningIndex];
            
            showResult(winner);
            
            // Çarkın tekrar açılmasını engelle
            localStorage.setItem(wheelStorageKey, 'spun');
            
        }, 4000);
    });
    
    function showResult(winner) {
        const resultDiv = document.getElementById('wheelResult');
        const resultText = document.getElementById('wheelResultText');
        const resultCode = document.getElementById('wheelResultCode');
        
        resultDiv.style.display = 'block';
        
        if (winner.code === '') {
            resultText.innerHTML = "Maalesef bu sefer boş geçtiniz. <br>Şansınızı başka sefere denemek üzere!";
            resultCode.style.display = 'none';
        } else {
            resultText.innerHTML = `Tebrikler! <strong>${winner.label}</strong> kazandınız! <br>Aşağıdaki kodu ödeme adımında kullanabilirsiniz:`;
            resultCode.style.display = 'inline-block';
            resultCode.innerText = winner.code;
        }
        
        // Kapatma butonunun metnini değiştir
        document.getElementById('closeWheelBtn').innerText = "Anladım, Kapat";
    }
});
