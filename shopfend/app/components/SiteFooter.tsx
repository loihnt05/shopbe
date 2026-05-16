export default function SiteFooter() {
  return (
    <footer className="bg-white border-t-4 border-[#ee4d2d] pt-12 pb-8 mt-12 text-[#000000a6] text-xs">
      <div className="sb-container px-4">
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4 mb-8">
          <div>
            <h3 className="font-bold mb-4 text-black/80">CUSTOMER SERVICE</h3>
            <ul className="space-y-3">
              <li><a href="#" className="hover:text-[#ee4d2d]">Help Centre</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Shopbee Blog</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">How To Buy</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">How To Sell</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Payment</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Shopbee Coins</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Shipping</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Return & Refund</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Contact Us</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Warranty Policy</a></li>
            </ul>
          </div>
          <div>
            <h3 className="font-bold mb-4 text-black/80">ABOUT SHOPBEE</h3>
            <ul className="space-y-3">
              <li><a href="#" className="hover:text-[#ee4d2d]">About Us</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Shopbee Careers</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Shopbee Policies</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Privacy Policy</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Shopbee Mall</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Seller Centre</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d]">Flash Deals</a></li>
            </ul>
          </div>
          <div>
            <h3 className="font-bold mb-4 text-black/80">PAYMENT</h3>
            <div className="flex flex-wrap gap-2 mb-4">
              <div className="bg-white p-1.5 rounded-sm shadow-sm border border-gray-200">Visa</div>
              <div className="bg-white p-1.5 rounded-sm shadow-sm border border-gray-200">Mastercard</div>
              <div className="bg-white p-1.5 rounded-sm shadow-sm border border-gray-200">JCB</div>
            </div>
            <h3 className="font-bold mb-4 text-black/80 mt-6">LOGISTICS</h3>
            <div className="flex flex-wrap gap-2">
              <div className="bg-white p-1.5 rounded-sm shadow-sm border border-gray-200">Shopbee Express</div>
              <div className="bg-white p-1.5 rounded-sm shadow-sm border border-gray-200">J&T Express</div>
            </div>
          </div>
          <div>
            <h3 className="font-bold mb-4 text-black/80">FOLLOW US</h3>
            <ul className="space-y-3">
              <li><a href="#" className="hover:text-[#ee4d2d] flex items-center gap-2">Facebook</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d] flex items-center gap-2">Instagram</a></li>
              <li><a href="#" className="hover:text-[#ee4d2d] flex items-center gap-2">LinkedIn</a></li>
            </ul>
          </div>
          <div>
            <h3 className="font-bold mb-4 text-black/80">APP DOWNLOAD</h3>
            <div className="flex gap-2">
              <div className="w-20 h-20 bg-gray-100 border border-gray-200 p-1">
                {/* QR Code Placeholder */}
                <div className="w-full h-full bg-white flex items-center justify-center text-[10px] text-center">QR Code</div>
              </div>
              <div className="flex flex-col gap-2 justify-center">
                <div className="bg-white border border-gray-200 px-2 py-1 text-[10px]">App Store</div>
                <div className="bg-white border border-gray-200 px-2 py-1 text-[10px]">Google Play</div>
              </div>
            </div>
          </div>
        </div>
        
        <div className="border-t border-black/10 mt-8 pt-8 flex justify-between items-center text-xs">
          <div>© 2026 Shopbee. All Rights Reserved.</div>
          <div>Country & Region: Singapore | Indonesia | Taiwan | Thailand | Malaysia | Vietnam | Philippines</div>
        </div>
      </div>
    </footer>
  );
}