export default function SiteFooter() {
  return (
    <footer className="bg-slate-50 border-t border-gray-200 pt-16 pb-10 mt-20 text-gray-500 text-[13px]">
      <div className="sb-container px-4">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-10 mb-12">
          <div className="lg:col-span-1">
            <h3 className="font-bold mb-5 text-gray-900 uppercase tracking-wider text-xs">Customer Service</h3>
            <ul className="space-y-3">
              <li><a href="#" className="hover:text-brand transition-colors">Help Centre</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Shopbee Blog</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">How To Buy</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">How To Sell</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Payment Methods</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Shipping & Delivery</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Return & Refund</a></li>
            </ul>
          </div>
          <div>
            <h3 className="font-bold mb-5 text-gray-900 uppercase tracking-wider text-xs">About Shopbee</h3>
            <ul className="space-y-3">
              <li><a href="#" className="hover:text-brand transition-colors">Our Story</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Careers</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Policies</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Privacy</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Shopbee Mall</a></li>
              <li><a href="#" className="hover:text-brand transition-colors">Flash Deals</a></li>
            </ul>
          </div>
          <div>
            <h3 className="font-bold mb-5 text-gray-900 uppercase tracking-wider text-xs">Payment</h3>
            <div className="flex flex-wrap gap-2 mb-6">
              {['Visa', 'Mastercard', 'JCB', 'Apple Pay', 'Google Pay'].map(p => (
                <div key={p} className="bg-white px-3 py-1.5 rounded-md shadow-sm border border-gray-100 text-[10px] font-bold text-gray-700">{p}</div>
              ))}
            </div>
            <h3 className="font-bold mb-5 text-gray-900 uppercase tracking-wider text-xs mt-8">Logistics</h3>
            <div className="flex flex-wrap gap-2">
              {['Shopbee Express', 'J&T', 'Grab', 'NinjaVan'].map(l => (
                <div key={l} className="bg-white px-3 py-1.5 rounded-md shadow-sm border border-gray-100 text-[10px] font-bold text-gray-700">{l}</div>
              ))}
            </div>
          </div>
          <div>
            <h3 className="font-bold mb-5 text-gray-900 uppercase tracking-wider text-xs">Follow Us</h3>
            <ul className="space-y-3">
              <li><a href="#" className="hover:text-brand transition-colors flex items-center gap-2">Facebook</a></li>
              <li><a href="#" className="hover:text-brand transition-colors flex items-center gap-2">Instagram</a></li>
              <li><a href="#" className="hover:text-brand transition-colors flex items-center gap-2">Twitter</a></li>
              <li><a href="#" className="hover:text-brand transition-colors flex items-center gap-2">TikTok</a></li>
            </ul>
          </div>
          <div>
            <h3 className="font-bold mb-5 text-gray-900 uppercase tracking-wider text-xs">Mobile App</h3>
            <div className="flex gap-4">
              <div className="w-24 h-24 bg-white rounded-xl shadow-md border border-gray-100 p-2 shrink-0">
                <div className="w-full h-full bg-slate-50 rounded-lg flex items-center justify-center text-[10px] text-center p-1 text-gray-400 font-medium">Scan to Download</div>
              </div>
              <div className="flex flex-col gap-2 justify-center">
                <div className="bg-gray-900 text-white px-4 py-2 rounded-lg text-[11px] font-medium flex items-center gap-2 hover:bg-gray-800 transition-colors cursor-pointer">
                  <span>App Store</span>
                </div>
                <div className="bg-gray-900 text-white px-4 py-2 rounded-lg text-[11px] font-medium flex items-center gap-2 hover:bg-gray-800 transition-colors cursor-pointer">
                  <span>Google Play</span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="border-t border-gray-200 pt-10 flex flex-col md:flex-row justify-between items-center gap-4 text-xs font-medium text-gray-400">
          <div>© 2026 Shopbee. Designed for a better shopping experience.</div>
          <div className="flex flex-wrap justify-center gap-x-4 gap-y-2 uppercase tracking-tight">
            <span>Singapore</span>
            <span className="text-gray-200">|</span>
            <span>Indonesia</span>
            <span className="text-gray-200">|</span>
            <span>Taiwan</span>
            <span className="text-gray-200">|</span>
            <span>Thailand</span>
            <span className="text-gray-200">|</span>
            <span>Malaysia</span>
            <span className="text-gray-200">|</span>
            <span>Vietnam</span>
            <span className="text-gray-200">|</span>
            <span>Philippines</span>
          </div>
        </div>
      </div>
    </footer>
  );
}