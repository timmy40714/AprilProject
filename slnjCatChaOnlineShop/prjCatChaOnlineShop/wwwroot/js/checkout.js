//優惠券選擇，當使用者選擇該優惠券時將優惠券名稱填至input標籤裡
$(document).ready(function () {
    $('.useCouponBtn').on('click', function () {
        // 獲取按鈕上的優惠券名稱
        var couponName = $(this).data('coupon-code');
        console.log(couponName);
        // 將優惠券名稱填入<input>標籤
        $("#couponCodeInput").val(couponName);
         // 模擬使用者操作關閉模態框
        $('.btn-close').click();
    });
});


//同意退換貨條款的選項為必勾選的項目，若沒有勾選擇無法送出訂單
$(document).ready(function () {
    // 監聽 checkbox 變更事件
    $('#returnsInvoice').change(function () {
        // 檢查 checkbox 是否被勾選
        if ($(this).is(':checked')) {
            // 隱藏錯誤訊息
            $('#error-message').text('');
        } else {
            // 顯示錯誤訊息
            $('#error-message').text('此為必勾選的項目');
        }
    });

    // 監聽送出訂單按鈕的點擊事件
    $('#submit-order-btn').click(function (e) {
        // 檢查 checkbox 是否被勾選
        if (!($('#returnsInvoice').is(':checked'))) {
            // 如果未勾選，取消點擊事件，防止送出訂單
            e.preventDefault();
            // 顯示錯誤訊息
            $('#error-message').text('此為必勾選的項目');
        }
    });
});

