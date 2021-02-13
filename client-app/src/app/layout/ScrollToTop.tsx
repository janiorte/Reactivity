import { useEffect } from "react";
import { useLocation, withRouter } from "react-router-dom";

export default withRouter(function ScrollToTop() {
  const { pathname } = useLocation();

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [pathname]);

  return null;
});